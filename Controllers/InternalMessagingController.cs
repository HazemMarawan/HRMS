using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRMS.Models;
using HRMS.ViewModels;
using HRMS.Auth;
using HRMS.Helpers;
using HRMS.Enums;
using System.IO;

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class InternalMessagingController : BaseController
    {
        // GET: InternalMessaging
        HRMSDBContext db = new HRMSDBContext();
        // GET: Mail
        public ActionResult Index()
        {
            User currentUser = Session["user"] as User;
            if(!isA.SuperAdmin())
                ViewBag.toUsers = db.Users.Where(s => s.id != currentUser.id && (s.branch_id == currentUser.branch_id || s.type == (int)UserRole.SuperAdmin)).Select(s => new { s.id, s.full_name }).ToList();
            else
                ViewBag.toUsers = db.Users.Where(s => s.id != currentUser.id).Select(s => new { s.id, s.full_name }).ToList();

            List<EmailViewModel> inboxMails = (from user in db.Users
                                               join email in db.Emails on user.id equals email.from_user
                                               join emailUser in db.EmailUsers on email.id equals emailUser.email_id
                                               select new EmailViewModel
                                               {
                                                   id = email.id,
                                                   from_user = email.from_user,
                                                   subject = email.subject,
                                                   body = email.body,
                                                   stringCreatedAt = email.created_at.ToString(),
                                                   created_at = email.created_at,
                                                   stringFromUser = user.full_name,
                                                   to_user = emailUser.user_id,
                                                   userImage = user.image,
                                                   emailAttachments = db.EmailAttachments.Where(e => e.email_id == email.id).Select(e => new EmailAttachmentViewModel
                                                   {
                                                       attachmentPath = e.attachmentPath
                                                   }).ToList()
                                               }).Where(s => s.to_user == currentUser.id).OrderByDescending(s => s.created_at).ToList();

            List<EmailViewModel> sendMails = (from email in db.Emails
                                              join emailUser in db.EmailUsers on email.id equals emailUser.email_id
                                              join user in db.Users on emailUser.user_id equals user.id
                                              select new EmailViewModel
                                              {
                                                  id = email.id,
                                                  subject = email.subject,
                                                  body = email.body,
                                                  stringCreatedAt = email.created_at.ToString(),
                                                  created_at = email.created_at,
                                                  stringToUser = user.full_name,
                                                  to_user = emailUser.user_id,
                                                  userImage = user.image,
                                                  from_user = email.from_user,
                                                  emailAttachments = db.EmailAttachments.Where(e => e.email_id == email.id).Select(e => new EmailAttachmentViewModel
                                                  {
                                                      attachmentPath = e.attachmentPath
                                                  }).ToList()
                                              }).Where(s => s.from_user == currentUser.id).OrderByDescending(s => s.created_at).ToList();
            MailboxViewModel mailboxViewModel = new MailboxViewModel();
            mailboxViewModel.inboxMails = inboxMails;
            mailboxViewModel.sendMails = sendMails;

            ViewBag.currentUserName = currentUser.full_name;

            return View(mailboxViewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult sendMail(EmailViewModel emailVM)
        {
            User currentUser = Session["user"] as User;
            Email email = AutoMapper.Mapper.Map<EmailViewModel, Email>(emailVM);

            email.from_user = currentUser.id;

            email.updated_at = DateTime.Now.AddHours(-3);
            email.created_at = DateTime.Now.AddHours(-3);

            db.Emails.Add(email);
            db.SaveChanges();

            if (email.related_id != null)
            {
                Email OriginalEmail = db.Emails.Find(email.related_id);
                if (OriginalEmail != null)
                {
                    OriginalEmail.related_id = email.id;
                    db.Entry(OriginalEmail).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }

            if (emailVM.to_users.Count() != 0)
            {
                foreach (int userId in emailVM.to_users)
                {
                    EmailUser emailUser = new EmailUser();
                    emailUser.email_id = email.id;
                    emailUser.user_id = userId;
                    emailUser.is_recieved = 0;
                    emailUser.created_at = DateTime.Now.AddHours(-3);
                    emailUser.updated_at = DateTime.Now.AddHours(-3);
                    db.EmailUsers.Add(emailUser);
                    db.SaveChanges();

                }
            }
            if (emailVM.attachments[0] != null)
            {
                foreach (var file in emailVM.attachments)
                {
                    Guid guid = Guid.NewGuid();
                    var InputFileName = Path.GetFileName(file.FileName);
                    var ServerSavePath = Path.Combine(Server.MapPath("~/Uploads/Email/Attachments/") + guid.ToString() + "attachment" + Path.GetExtension(file.FileName));
                    file.SaveAs(ServerSavePath);

                    EmailAttachment emailAttachment = new EmailAttachment();
                    emailAttachment.attachmentPath = "/Uploads/Email/Attachments/" + guid.ToString() + "attachment" + Path.GetExtension(file.FileName);
                    emailAttachment.email_id = email.id;

                    db.EmailAttachments.Add(emailAttachment);
                    db.SaveChanges();
                }
            }

            return Redirect("/InternalMessaging/Index");

        }
    }
}