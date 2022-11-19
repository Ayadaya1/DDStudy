using AutoMapper;
using DAL;
using Api.Models;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class SubscriptionService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;

        public SubscriptionService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        
    }
}
