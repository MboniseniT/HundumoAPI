using BinmakAPI.Data;
using BinmakBackEnd.Entities;
using BinmakBackEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Services
{
    public class AssetNodeService
    {
        private readonly BinmakDbContext _context;

        public AssetNodeService(BinmakDbContext context)
        {
            _context = context;
        }
        public List<AssetNodeVM> GetAssetNodeVMs(string reference)
        {



            List<UserGroup> userGroups = _context.UserGroups.Where(id => id.UserId == reference).ToList();
            List<int> userGroupIds = new List<int>();

            foreach (var item in userGroups)
            {
                userGroupIds.Add(item.GroupId);
            }

            List<Group> groups = new List<Group>();

            foreach (var gId in userGroupIds)
            {

                groups.Add(_context.Groups.FirstOrDefault(id => id.GroupId == gId));
            }

            List<AssetNode> lAssetNodes = new List<AssetNode>();
            foreach (var g in groups)
            {
                AssetNode assetNode = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == g.AssetNodeId/*) && (id.ParentAssetNodeId != 0)*/);
                lAssetNodes.Add(assetNode);
            }

            List<AssetNode> assetNodes = new List<AssetNode>();

            foreach (var item in lAssetNodes)
            {
                assetNodes.AddRange(_context.AssetNodes.Where(id => id.AssetNodeId == item.AssetNodeId).ToList());
                //Add First Node as well
                //assetNodes.AddRange(_context.AssetNodes.Where(id => id.AssetNodeId == item.AssetNodeId).ToList());
            }

            assetNodes.OrderBy(a => a.Height);


            //var assetNodes = _context.AssetNodes.Where(a => a.Reference.Equals(reference)).OrderBy(a => a.Height).ToList();

            List<AssetNodeVM> assetNodesVM = new List<AssetNodeVM>();

            foreach (var item in assetNodes)
            {
                assetNodesVM.Add(new AssetNodeVM() { Code = item.Code, RootAssetNodeId = item.RootAssetNodeId, DateStamp = item.DateStamp, 
                    Name = item.Name + " ("+ _context.AssetNodeTypes.FirstOrDefault(id => id.AssetNodeTypeId == item.AssetNodeTypeId).AssetNodeTypeName +")", 
                    AssetNodeId = item.AssetNodeId, ParentAssetNodeId = item.ParentAssetNodeId, Reference = item.Reference, Height = item.Height, NodeId = item.AssetNodeId, 
                    NodeType = item.AssetNodeTypeId /*Type = 1 if organization*/ });
            }

            List<AssetNodeVM> assetNodesVMOrdered = assetNodesVM.OrderBy(id => id.AssetNodeId).ToList();

            return assetNodesVMOrdered;
        }
    }
}
