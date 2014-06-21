//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineShopTime.Models
{
    using CloudinaryDotNet;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Offers
    {
        public Offers()
        {
            this.Comments = new HashSet<Comments>();
            this.OfferRaiting = new HashSet<OfferRaiting>();
            this.Orders = new HashSet<Orders>();
            this.Tags = new HashSet<Tags>();


            Account acc = new Account(
                    Properties.Settings.Default.CloudName,
                    Properties.Settings.Default.ApiKey,
                    Properties.Settings.Default.ApiSecret);

            Cloudinary = new Cloudinary(acc);
        }

        public Cloudinary Cloudinary { get; set; }
    
        public string OfferID { get; set; }
        [Required(ErrorMessage = "@Resources.Home.ErroeNameInput")]
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> DateAndTime { get; set; }
        public string OfferedBy { get; set; }
        public string Photo1URL { get; set; }
        public string Photo2URL { get; set; }
        public string Photo3URL { get; set; }
        public string Photo4URL { get; set; }
    
        public virtual ICollection<Comments> Comments { get; set; }
        public virtual ICollection<OfferRaiting> OfferRaiting { get; set; }
        public virtual Users Users { get; set; }
        public virtual ICollection<Orders> Orders { get; set; }
        public virtual ICollection<Tags> Tags { get; set; }
    }
}
