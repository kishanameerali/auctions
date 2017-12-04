using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using auctions.Models;
using System.Linq;

namespace auctions.Controllers
{
    public class AuctionController : Controller
    {
        private AuctionContext _context;
        private User ActiveUser
        {
            get { return _context.users.Where(u => u.userid == HttpContext.Session.GetInt32("user_id")).FirstOrDefault();}
        }

        public AuctionController(AuctionContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("dashboard")]
        public IActionResult Dashboard()
        {
            // check if user is logged in
            if(ActiveUser == null)
                return RedirectToAction("Index", "Home");

            // now we check to see if any auctions should be closed
            
            TimeSpan Zero = new TimeSpan(0,0,1);


            List <Auction> AllAuctions = _context.auctions.Where(a => a.auction_status == "active").ToList();

            foreach(Auction auc in AllAuctions)
            {
                TimeSpan current = auc.end_date.Subtract(DateTime.Now);
                if(current > Zero)
                {
                    auc.time_remaining = current.Days;
                    _context.SaveChanges();
                }
                else
                {
                    auc.time_remaining = Zero.Days;
                    auc.auction_status = "closed";

                    //now that auction is closed lets find out who submitted the winning bid
                    User seller = _context.users.SingleOrDefault(u => u.userid == auc.userid);
                    List<Bid> winning_bid = _context.bids.Where(b => b.auctionid == auc.auctionid)
                                                .Where(b => b.amount == auc.top_bid).ToList();
                    Bid winner_bid = winning_bid[0];

                    User winner = _context.users.SingleOrDefault(u => u.userid == winner_bid.userid);

                    //now lets update the wallets
                    seller.wallet = seller.wallet + winner_bid.amount;
                    winner.wallet = winner.wallet - winner_bid.amount;
                    _context.SaveChanges();
                }
            }

            return View(new ShowAuctions
            {
                User = ActiveUser,
                OpenAuctions = _context.auctions.OrderBy(a => a.time_remaining)
                    .Where(a => a.auction_status == "active")
                    .Include(a => a.user)
                    .Include(a => a.bids).ToList()
            });
        }

        //delete a auction that the active user has posted
        [HttpGet]
        [Route("delete/{auction_id}")]
        public IActionResult Delete(int auction_id)
        {
            if(ActiveUser == null)
                return RedirectToAction("Index", "Home");

            Auction myAuction = _context.auctions.SingleOrDefault(a => a.auctionid == auction_id);
            List<Bid> assoc_bids = _context.bids.Where(b => b.auctionid == myAuction.auctionid).ToList();
            
            foreach(Bid assoc_bid in assoc_bids)
            {
                _context.bids.Remove(assoc_bid);
            }
            _context.auctions.Remove(myAuction);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        [Route("new")]
        public IActionResult New()
        {
            if(ActiveUser == null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        //adding a auction
        [HttpPost]
        [Route("add")]
        public IActionResult Add(AuctionInfo model)
        {
            if(ActiveUser == null)
                return RedirectToAction("Index", "Home");
            
      

            TimeSpan Zero = new TimeSpan(0,0,0);
            Auction addedAuction = model.NewAuction;
            Bid firstBid = model.NewBid;
            TimeSpan start = addedAuction.end_date.Subtract(DateTime.Now);

            firstBid.auction = addedAuction;
            firstBid.user = ActiveUser;
            addedAuction.user = ActiveUser;
            addedAuction.top_bid = firstBid.amount;
            addedAuction.auction_status = "active";
            addedAuction.time_remaining = start.Days;

            if(firstBid.amount < 0.01)
            {
                ModelState.AddModelError("amount", "Starting bid is at least $0.01");
            }

            if(ModelState.IsValid)
            {
                _context.auctions.Add(addedAuction);
                _context.bids.Add(firstBid);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return View("New");
        }

        //display information for a single auction
        [HttpGet]
        [Route("auction/{auction_id}")]
        public IActionResult Show(int auction_id)
        {
            if(ActiveUser == null)
                return RedirectToAction("Index", "Home");

            if(TempData["errors"] != null)
            {
                ViewBag.Errors = TempData["errors"];
            }

            Auction currentAuction = _context.auctions.SingleOrDefault(a => a.auctionid == auction_id);
            User seller = _context.users.SingleOrDefault(u => u.userid == currentAuction.userid);
            List<Bid> currentBids = _context.bids.Where(b => b.auctionid == currentAuction.auctionid)
                                                .Where(b => b.amount == currentAuction.top_bid).ToList();
            
            return View(new AuctionInfo{
                NewAuction = currentAuction,
                NewBid = currentBids[0],
                NewUser = seller
            });
        }

        //submitting a bid
        [HttpPost]
        [Route("itembid")]
        public IActionResult ItemBid(AuctionInfo model)
        {
            if(ActiveUser == null)
                return RedirectToAction("Index", "Home");

            Bid currentBid = model.NewBid;
            currentBid.user = ActiveUser;
            Auction currentAuction = _context.auctions.SingleOrDefault(a => a.auctionid == currentBid.auctionid); 

            if(ActiveUser.wallet < currentBid.amount)
            {
                //ModelState.AddModelError("amount", "You do not have enough in your wallet");
                TempData["errors"] = "You do not have enough in your wallet";
                return Redirect($"auction/{currentAuction.auctionid}");
            }
            
            if(currentBid.amount <= currentAuction.top_bid)
            {
                //ModelState.AddModelError("amount", "Bid is too low");
                TempData["errors"] = "Bid is too low";
                return Redirect($"auction/{currentAuction.auctionid}");
            }
            if(currentAuction.auction_status == "closed")
            {
                //ModelState.AddModelError("amount", "This auction is closed");
                TempData["errors"] = "This auction is closed";
                return Redirect($"auction/{currentAuction.auctionid}");
                
            }
            if(ModelState.IsValid)
            {
                currentAuction.top_bid = currentBid.amount;
                _context.bids.Add(currentBid);
                _context.SaveChanges();
                //return RedirectToAction("Dashboard");
            }
            return RedirectToAction("Dashboard");
            //return View("Show");
        }
    }
}