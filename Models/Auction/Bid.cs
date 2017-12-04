using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using auctions.Models.Validations;

namespace auctions.Models
{
    public class Bid
    {
        [Key]
        public int bidid {get;set;}
        [Required]
        [Display(Name="Starting Bid")]
        public double amount {get;set;}
        public int auctionid {get;set;}
        public Auction auction {get;set;}
        public int userid {get;set;}
        public User user {get;set;}
    }
    
}