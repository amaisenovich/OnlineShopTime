﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineShopTime.Models;
using Microsoft.AspNet.Identity;

namespace OnlineShopTime.Controllers
{
    public class AdministratorController : Controller
    {
        //
        // GET: /Administrator/
        public ActionResult AdminPanel()
        {
            WorkWithUsers WWU = new WorkWithUsers();
            IQueryable<Users> Users = (IQueryable<Users>)Session["Users"];
            if (Users == null)
            {
                Users = WWU.GetWorseUsers();
                WWU.CheckUserRoles();
            }
            Session["Users"] = Users;
            if (User.Identity.Name != "")
                if (WWU.GetUserRole(IdentityExtensions.GetUserId(User.Identity)) == "Admin")
                    return View(Users);
            return RedirectToAction("AccessDenied", "AccessDenied");

        }
        public ActionResult SetRights(string UserID, string Rights)
        {
            WorkWithUsers WWU = new WorkWithUsers();
            WWU.SetUserRights(UserID, Rights);
            Session["Users"] = null;
            return RedirectToAction("AdminPanel");
        }
        public ActionResult Search(string SearchText)
        {
            WorkWithUsers WWU = new WorkWithUsers();
            if (SearchText != "")
            {
                IQueryable<Users> FiltredUsers = WWU.SearchInUsers(SearchText);
                Session["Users"] = FiltredUsers;
            }
            else
                Session["Users"] = WWU.GetWorseUsers();
            return RedirectToAction("AdminPanel");
        }
        public ActionResult ShowAdminContacts(string UserID)
        {
            WorkWithUsers WWU = new WorkWithUsers();
            return View(WWU.GetAdminsList());
        }
    }
}