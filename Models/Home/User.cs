using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace auctions.Models
{
    public class User
    {
        [Key]
        public int userid {get;set;}
        public string first_name {get;set;}
        public string last_name {get;set;}
        public string password {get;set;}
        public string username {get;set;}
        public double wallet {get;set;}
        public List<Auction> auctions {get;set;}
        public List<Bid> bids {get;set;}
        public User()
        {
            auctions = new List<Auction>();
            bids = new List<Bid>();
        }
    }
}