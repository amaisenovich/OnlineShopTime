﻿using OnlineShopTime.Utils;
using SimpleLucene.Impl;
using SimpleLucene.IndexManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;


namespace OnlineShopTime.Models
{
    public class WorkWithOffers
    {
        ShopDBEntities Db;
        HttpServerUtilityBase Server;

        public WorkWithOffers(HttpServerUtilityBase Server)
        {
            this.Server = Server;
            Db = new ShopDBEntities();
        }
        public string GetActiveUserID(string UserName)
        {
            return (from rec in Db.Users where rec.UserName == UserName select rec.UserID).FirstOrDefault();
        }
        public string GetAndDeleteCurrency(Offers Offer)
        {
            string Result = null;
            if (Offer.Price != null)
            {
                if (Offer.Price.Contains("RUB"))
                    Result = "RUB";
                if (Offer.Price.Contains("EUR"))
                    Result = "EUR";
                if (Offer.Price.Contains("CNY"))
                    Result = "CNY";
                if (Offer.Price.Contains("USD"))
                    Result = "USD";
                Offer.Price = Offer.Price.Substring(0, Offer.Price.Length - 4);                
            }
            return Result;
        }
        public IQueryable<Offers> GetTopOffers()
        {
            var OfferRate = from RateRec in Db.OfferRaiting
                            group RateRec.Raiting by RateRec.OfferID into RecGroup
                            select new { Key = RecGroup.Key, Rate = RecGroup.ToList().Sum() / RecGroup.Count() };

            IQueryable<Offers> TopOfferrs = (from OfferRec in Db.Offers
                                             join RateRec in OfferRate on OfferRec.OfferID equals RateRec.Key
                                             orderby RateRec.Rate descending
                                             select OfferRec).Take(12);

            return TopOfferrs;
        }
        public void AddTagsToOffer(string OfferID, string TagsString)
        {
            this.AddTagsToOffer((from OfferRecords in Db.Offers where OfferRecords.OfferID == OfferID select OfferRecords).FirstOrDefault(), TagsString);
        }
        public void AddTagsToOffer(Offers Offer, string TagsString)
        {
            string[] InputedTags = TagsString.Split(',');
            foreach (string str in InputedTags)
            {
                if (str != "")
                {
                    Tags Tag = null;
                    Tag = (from TagsRecords in Db.Tags where TagsRecords.Name == str select TagsRecords).FirstOrDefault();
                    if (Tag == null)
                    {
                        Tag = new Tags();
                        Tag.TagID = Guid.NewGuid().ToString();
                        Tag.Name = str;
                    }
                    Tag.Offers.Add(Offer);
                    Offer.Tags.Add(Tag);
                }
            }
            Db.SaveChanges();
        }
        public Offers CompleteOfferWithData(Offers newOffer, String UserName)
        {
            String UserID = GetActiveUserID(UserName);
            newOffer.Users = (from rec in Db.Users where rec.UserID == UserID select rec).FirstOrDefault();
            newOffer.DateAndTime = DateTime.Now;
            newOffer.OfferedBy = UserID;
            newOffer.OfferID = Guid.NewGuid().ToString();
            return newOffer;
        }
        public IQueryable<Offers> GetNewOffers()
        {
            return (from OfferRecord in Db.Offers orderby OfferRecord.DateAndTime descending select OfferRecord).Take(12);
        }
        public Offers GetOfferByID(String OfferID)
        {
            return (from OfferRecods in Db.Offers where OfferRecods.OfferID == OfferID select OfferRecods).FirstOrDefault();
        }
        public string AndNewOrModify(Offers Offer, string UserName)
        {
            string Result = null;
            int count = (from OfferRecords in Db.Offers where OfferRecords.OfferID == Offer.OfferID select OfferRecords).Count();
            if (count == 0)
            {
                Result = AddNewOffer(Offer, UserName);
            }
            else
                Result = ModifyExistedOffer(Offer);
            return Result;
        }
        public string ModifyExistedOffer(Offers EditedOffer)
        {
            Offers OldOne = (from OfferRecords in Db.Offers where OfferRecords.OfferID == EditedOffer.OfferID select OfferRecords).FirstOrDefault();
            OldOne.Name = EditedOffer.Name;
            OldOne.Description = EditedOffer.Description;
            OldOne.Price = EditedOffer.Price;
            OldOne.Photo1URL = EditedOffer.Photo1URL;
            OldOne.Photo2URL = EditedOffer.Photo2URL;
            OldOne.Photo3URL = EditedOffer.Photo3URL;
            OldOne.Photo4URL = EditedOffer.Photo4URL;
            Db.SaveChanges();
            return OldOne.OfferID;
        }
        public string AddNewOffer(Offers newOffer, string UserName)
        {
            newOffer = this.CompleteOfferWithData(newOffer, UserName);
            Db.Offers.Add(newOffer);
            Db.SaveChanges();
            return newOffer.OfferID;
        }


        public void CreateIndex(Offers newOffer)
        {
            // index location
            var indexLocation = new FileSystemIndexLocation(new DirectoryInfo(Server.MapPath("~/Index")));
            var definition = new OfferIndexDefinition();
            var task = new EntityUpdateTask<Offers>(newOffer, definition, indexLocation);
            task.IndexOptions.RecreateIndex = false;
            task.IndexOptions.OptimizeIndex = true;
            //IndexQueue.Instance.Queue(task);
            var indexWriter = new DirectoryIndexWriter(new DirectoryInfo(Server.MapPath("~/Index")), false);

            using (var indexService = new IndexService(indexWriter))
            {
                task.Execute(indexService);
            }
        }
        public IQueryable<Offers> GetUserOffers(String UserID, int PageNumber = 1)
        {
            return (from OffersRecords in Db.Offers orderby OffersRecords.DateAndTime descending where OffersRecords.OfferedBy == UserID select OffersRecords).Skip((PageNumber - 1) * 20).Take(20);
        }

        public void DeleteOffer(string OfferID)
        {
            Offers RemoveOffer = (from OfferRecords in Db.Offers where OfferRecords.OfferID == OfferID select OfferRecords).FirstOrDefault();
            DeleteOfferRaiting(OfferID);
            DeleteOfferTags(OfferID);
            DeleteOfferComments(OfferID);
            DeleteOfferOrders(OfferID);
            Db.Offers.Remove(RemoveOffer);
            Db.SaveChanges();
        }
        private void DeleteOfferOrders(string OfferID)
        {
            IQueryable<Orders> OfferOrders = from OrdersRecords in Db.Orders where OrdersRecords.OfferID == OfferID select OrdersRecords;
            if (OfferOrders != null)
                foreach (Orders Order in OfferOrders)
                    Db.Orders.Remove(Order);
            Db.SaveChanges();
        }
        private void DeleteOfferComments(string OfferID)
        {
            IQueryable<Comments> OfferCommnets = from CommentsRecords in Db.Comments where CommentsRecords.OfferID == OfferID select CommentsRecords;
            if (OfferCommnets != null)
                foreach (Comments Comment in OfferCommnets)
                    Db.Comments.Remove(Comment);
            Db.SaveChanges();
        }
        public void DeleteOfferTags(string OfferID)
        {
            Offers Offer = (from OffersRecords in Db.Offers where OffersRecords.OfferID == OfferID select OffersRecords).FirstOrDefault();
            ICollection<Tags> OfferTags = (from OfferRecords in Db.Offers where OfferRecords.OfferID == OfferID select OfferRecords.Tags).FirstOrDefault();
            if (OfferTags != null)
            {
                foreach (Tags Tag in OfferTags)
                {
                    Offer.Tags.Remove(Tag);
                    Tag.Offers.Remove(Offer);
                }
                Db.SaveChanges();
            }
        }
        private void DeleteOfferRaiting(string OfferID)
        {
            IQueryable<OfferRaiting> OfferRate = from RateRec in Db.OfferRaiting where RateRec.OfferID == OfferID select RateRec;
            foreach (OfferRaiting OR in OfferRate)
                Db.OfferRaiting.Remove(OR);
            Db.SaveChanges();
        }
    }
}