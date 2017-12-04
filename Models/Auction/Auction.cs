using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using auctions.Models.Validations;

namespace auctions.Models
{
    public class Auction
    {
        [Key]
        public int auctionid {get;set;}
        [Required]
        [MinLength(4, ErrorMessage = "Product Name must be more than 4 characters")]
        [Display(Name="Product Name")]
        public string product {get;set;}
        [Required]
        [MinLength(11, ErrorMessage = "Description must be more than 10 characters")]
        [Display(Name="Description")]
        public string description {get;set;}
        public double top_bid {get;set;}
        [Required]
        [FutureDate]
        [Display(Name="End Date")]
        [DataType(DataType.Date)]
        public DateTime end_date {get;set;}
        public int time_remaining {get;set;}
        public string auction_status {get;set;}
        public int userid {get;set;}
        public User user {get;set;}
        public List<Bid> bids {get;set;}

        public Auction()
        {
            bids = new List<Bid>();
        }
    }
}