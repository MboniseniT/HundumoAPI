using BinmakAPI.Data;
using BinmakBackEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Services
{

    public class OrganizationService
    {
        private readonly BinmakDbContext _context;

        public OrganizationService(BinmakDbContext context)
        {
            _context = context;
        }
        public List<OrganizationVM> GetOrganizationVMs(string reference)
        {
            //var orginisations = _context.Organizations.Where(a => a.Reference.Equals(reference)).OrderBy(a => a.Height).ToList();
            //var productiveUnit = _context.ProductiveUnits.Where(a => a.Reference.Equals(reference)).OrderBy(a => a.Height).ToList();
            //var equipment = _context.Equipments.Where(a => a.Reference.Equals(reference)).OrderBy(a => a.Height).ToList();

            List<OrganizationVM> orginisationsVM = new List<OrganizationVM>();

            //foreach (var item in orginisations)
            //{
            //    orginisationsVM.Add(new OrganizationVM() { Code = item.Code, RootOrganizationId = item.RootOrganizationId, 
            //        DateStamp = item.DateStamp, Name = string.Format("{0} - [Organization]" ,item.Name), OrganizationId = item.OrganizationId, 
            //        ParentOrganizationId = item.ParentOrganizationId, Reference = item.Reference, Height = item.Height, NodeId = item.OrganizationId, 
            //        NodeType = 1 /*Type = 1 if organization*/ });
            //}
            //foreach (var item in productiveUnit)
            //{
            //    orginisationsVM.Add(new OrganizationVM() { Code = item.Code, RootOrganizationId = item.RootOrganizationId, DateStamp = item.DateStamp, 
            //        Name = string.Format("{0} - [Productive Unit]", item.Name), OrganizationId = item.ProductiveUnitId, ParentOrganizationId = item.ParentProductiveUnitId, 
            //        Reference = item.Reference, Height = item.Height, NodeId = item.ProductiveUnitId, NodeType = 2 /*Type = 1 if productive unit*/ });
            //}
            //foreach (var item in equipment)
            //{
            //    orginisationsVM.Add(new OrganizationVM() { Code = item.Code, RootOrganizationId = item.RootOrganizationId, DateStamp = item.DateStamp, 
            //        Name = string.Format("{0} - [Equipment]", item.Name), OrganizationId = item.EquipmentId, ParentOrganizationId = item.ParentEquipmentId, 
            //        Reference = item.Reference, Height = item.Height, NodeId = item.EquipmentId, NodeType = 3 /*Type = 1 if equipment*/ });
            //}
            return orginisationsVM;
        }

    }
}
