using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace auctions.Models
{
    public class ShowAuctions
    {
        public User User {get;set;}
        public List<Auction> OpenAuctions {get;set;}
    }

    public class AuctionInfo
    {
        public Auction NewAuction {get;set;}
        public Bid NewBid {get;set;}
        public User NewUser {get;set;}
    }
}