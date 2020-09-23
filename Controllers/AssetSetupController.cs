using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinmakAPI.Data;
using BinmakBackEnd.Areas.ProductionFlow.Entities;
using BinmakBackEnd.Entities;
using BinmakBackEnd.Models;
using BinmakBackEnd.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BinmakBackEnd.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class AssetSetupController : ControllerBase
    {
        private readonly BinmakDbContext _context;

        public AssetSetupController(BinmakDbContext context)
        {
            _context = context;
        }

        [HttpGet("organization")]
        public IActionResult GetOrganization(string reference)
        {
            if (reference == "")
            {
                return BadRequest("Something bad happened, Log out and Log in and try again");
            }

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
            }

            var lAssetNode = assetNodes.Where(id => id.AssetNodeTypeId == 1).ToList();

            return Ok(lAssetNode);
        }

        [HttpGet("productiveUnit")]
        public IActionResult GetProductiveUnits(string reference)
        {
            if (reference == "")
            {
                return BadRequest("Something bad happened, Log out and Log in and try again");
            }

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

            List<AssetNode> lAssNodes = assetNodes.Where(id => id.AssetNodeTypeId != 3).ToList();

            return Ok(lAssNodes);
        }

        //[HttpGet("productiveUnitByOrganazation")]
        //public IActionResult GetProductiveUnitByOrganization(int organizationId)
        //{

        //    List<ProductiveUnit> organizations = _context.ProductiveUnits.Where(id => id.OrganizationId == organizationId).ToList();

        //    return Ok(organizations);
        //}

        [HttpGet("EquipmentByProductiveUnit")]
        public IActionResult GetEquipmentByProductiveUnit(/*int productiveUnitId*/ string reference)
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

            List<AssetNode> lAssNodes = assetNodes.Where(id => id.AssetNodeTypeId != 1).ToList();

            return Ok(lAssNodes);
        }

        [HttpGet("GetAssetUsers")]
        public IActionResult GetAssetUsers(/*int productiveUnitId*/ string reference)
        {
            List<AssetNode> assetNodes = _context.AssetNodes.Where(id => (id.Reference == reference) && (id.AssetNodeTypeId != 3)).ToList();

            return Ok(assetNodes);
        }

        [HttpPost("editOrganization")]
        public IActionResult EditOrganization([FromBody] AssetNode assetNode)
        {
            if (assetNode == null)
            {
                return BadRequest("Something bad happened. Make sure form is filled correctly");
            }

            try
            {
                AssetNode model = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == assetNode.AssetNodeId);
                model.Name = assetNode.Name.ToUpper();
                model.ParentAssetNodeId = assetNode.ParentAssetNodeId;
                model.LastEditedBy = assetNode.LastEditedBy;
                model.LastEditedDate = DateTime.Now;
                model.Address = assetNode.Address;
                model.Address2 = assetNode.Address2;
                model.City = assetNode.City;
                model.Code = assetNode.Code;
                model.CountryId = assetNode.CountryId;

                _context.AssetNodes.Update(model);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened." + Ex.Message);
            }
        }

        [HttpPost("assetNodes")]
        public IActionResult PostOrganization([FromBody] AssetNode assetNode)
        {
            if (assetNode == null)
            {
                return BadRequest("Something bad happened. Object is null");
            }

            AssetNode assetNodeExistChecker = _context.AssetNodes.FirstOrDefault(id => (id.Code == assetNode.Code) && (id.RootAssetNodeId == assetNode.RootAssetNodeId));

            if (assetNodeExistChecker != null)
            {
                return BadRequest("There is already an asset node with existing CODE within the root asset node, choose a different code.");
            }

            try
            {
                AssetNode model = new AssetNode();
                model.ParentAssetNodeId = assetNode.ParentAssetNodeId;
                model.Name = assetNode.Name.ToUpper();
                model.DateStamp = DateTime.Now;
                model.Reference = assetNode.Reference;
                model.AssetNodeTypeId = assetNode.AssetNodeTypeId;

                model.Code = assetNode.Code.ToUpper();

                if (model.ParentAssetNodeId == 0)
                {
                    model.RootAssetNodeId = 0;
                }

                if (assetNode.IsParentAddress)
                {
                    if (model.ParentAssetNodeId != 0)
                    {
                        model.ParentAssetNodeId = assetNode.ParentAssetNodeId;
                    }
                    else
                    {
                        model.ParentAssetNodeId = 0;
                    }

                    AssetNode parentAssetNode = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == assetNode.ParentAssetNodeId);
                    model.Address = parentAssetNode.Address;
                    model.Address2 = parentAssetNode.Address2;
                    model.City = parentAssetNode.City;
                    model.Zip = parentAssetNode.Zip;
                    model.CountryId = parentAssetNode.CountryId;
                }
                else
                {
                    if (model.ParentAssetNodeId != 0)
                    {
                        model.ParentAssetNodeId = assetNode.ParentAssetNodeId;
                    }
                    else
                    {
                        model.ParentAssetNodeId = 0;
                    }

                    model.Address = assetNode.Address;
                    model.Address2 = assetNode.Address2;
                    model.City = assetNode.City;
                    model.Zip = assetNode.Zip;
                    model.CountryId = assetNode.CountryId;
                }

                _context.AssetNodes.Add(model);
                _context.SaveChanges();

                var isRoot = true;

                AssetNode root = new AssetNode();

                if (model.ParentAssetNodeId == 0)
                {
                    model.RootAssetNodeId = 0;
                }
                else
                {
                    List<UserGroup> userGroups = _context.UserGroups.Where(id => id.UserId == model.Reference).ToList();
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
                        AssetNode assetNode1 = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == g.AssetNodeId/*) && (id.ParentAssetNodeId != 0)*/);
                        lAssetNodes.Add(assetNode1);
                    }

                    List<AssetNode> assetNodes = new List<AssetNode>();

                    foreach (var item in lAssetNodes)
                    {
                        assetNodes.AddRange(_context.AssetNodes.Where(id => id.AssetNodeId == item.AssetNodeId).ToList());
                    }

                    root = assetNodes.FirstOrDefault(id => /*(*/id.AssetNodeId == assetNode.ParentAssetNodeId/*) && (id.Reference == assetNode.Reference)*/);

                    while (isRoot)
                    {
                        if (root.Height == 1)
                            isRoot = false;
                        else
                            root = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == root.ParentAssetNodeId);
                    }
                    model.RootAssetNodeId = root.AssetNodeId;

                }



                if (model.ParentAssetNodeId == 0)
                {
                    model.Height = model.Height + 1;
                }
                else
                {
                    model.Height = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == model.ParentAssetNodeId).Height + 1;
                }


                //If productive unit, it must access production flow, and subsequent parent id should the id of the clientAsset name

                /*if (model.AssetNodeTypeId == 2) //Productive Unit
                {
                    ClientAsset clientAssetName = new ClientAsset();
                    clientAssetName.AssetName = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == model.ParentAssetNodeId).Name;
                    clientAssetName.DateStamp = DateTime.Now;
                    clientAssetName.Reference = model.Reference;
                    clientAssetName.ClientName = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == model.RootAssetNodeId).Name;
                    clientAssetName.ClientAssetNameId = model.ParentAssetNodeId;

                    _context.ClientAssetNames.Add(clientAssetName);
                    _context.SaveChanges();

                    //Add Asset, from a production's flow perspective
                    BinmakBackEnd.Areas.ProductionFlow.Entities.ProductionFlowAsset productionFlowAsset = new Areas.ProductionFlow.Entities.ProductionFlowAsset();
                    productionFlowAsset.AssetId = model.AssetNodeId;
                    productionFlowAsset.ClientAssetNameId = model.ParentAssetNodeId;
                    productionFlowAsset.SiteName = model.Name;
                    productionFlowAsset.Reference = model.Reference;
                    productionFlowAsset.TemplateId = 1;
                    productionFlowAsset.SinceDateProduction = DateTime.Now;
                    productionFlowAsset.DateStamp = DateTime.Now;
                    _context.ProductionFlowAssets.Add(productionFlowAsset);
                    _context.SaveChanges();

                    List<FunctionUnit> functionUnits = saveAssetFunctionUnits(productionFlowAsset);
                    List<FunctionUnitChildren> functionUnitsChildren = saveAssetFunctionUnitsChildren(productionFlowAsset);

                    List<DateTime> dates = getAllDates(productionFlowAsset.SinceDateProduction.Year, productionFlowAsset.SinceDateProduction.Month);
                    List<Reading> readings = new List<Reading>();

                    foreach (var date in dates)
                    {
                        Reading reading = new Reading();
                        reading.DateProduction = date;
                        reading.AssetId = productionFlowAsset.AssetId;
                        reading.Reference = productionFlowAsset.Reference;

                        readings.Add(reading);

                    }

                    _context.Readings.AddRange(readings);
                    _context.SaveChanges();

                }*/

                //Add Group


                _context.SaveChanges();
                /*if (model.RootAssetNodeId == 0)
                {*/
                    Group group = new Group();
                    group.Reference = assetNode.Reference;
                    group.GroupName = assetNode.Name.ToUpper() + " [" + assetNode.Code+"]".ToUpper();
                    group.DateStamp = DateTime.Now;
                    group.AssetNodeId = model.AssetNodeId;

                    if (model.RootAssetNodeId == 0)
                    {
                        group.RootId = model.AssetNodeId;
                    }
                    else
                    {
                        group.RootId = model.RootAssetNodeId;
                    }
               

                    _context.Groups.Add(group);
                    _context.SaveChanges();

                    List<string> userIds = new List<string>();
                    List<UserGroup> userGroups1 = _context.UserGroups.Where(id => id.RootId == group.RootId).ToList();
                    foreach (var ug in userGroups1)
                    {
                    userIds.Add(ug.UserId);
                    }

                    var distinctUsers = userIds.Distinct().ToList();

                    if (userGroups1.Count > 0)
                    {
                        foreach (var userId in distinctUsers)
                        {
                            UserGroup userGroup = new UserGroup();
                            userGroup.GroupId = group.GroupId;
                            userGroup.UserId = userId;

                            if (model.RootAssetNodeId == 0)
                            {
                            userGroup.RootId = model.AssetNodeId;
                            }
                            else
                            {
                                userGroup.RootId = model.RootAssetNodeId;
                            }
                    
                            model.GroupId = group.GroupId;
                            _context.AssetNodes.Update(model);
                            _context.SaveChanges();

                            _context.UserGroups.Add(userGroup);
                            _context.SaveChanges();
                        }
                    }
                        else
                        {
                            UserGroup userGroup = new UserGroup();
                            userGroup.GroupId = group.GroupId;
                            userGroup.UserId = model.Reference;

                            if (model.RootAssetNodeId == 0)
                            {
                                userGroup.RootId = model.AssetNodeId;
                            }
                            else
                            {
                                userGroup.RootId = model.RootAssetNodeId;
                            }

                            model.GroupId = group.GroupId;
                            _context.AssetNodes.Update(model);
                            _context.SaveChanges();

                            _context.UserGroups.Add(userGroup);
                            _context.SaveChanges();
                    }




                /*}
                else
                {
                    var parent = _context.AssetNodes.FirstOrDefault(id=>id.AssetNodeId == model.ParentAssetNodeId);
                    List<UserGroup> assetUsers = _context.UserGroups.Where(id => id.GroupId == parent.GroupId).ToList();

                    foreach (var au in assetUsers)
                    {
                        UserGroup userGroup1 = new UserGroup();
                        userGroup1.GroupId = au.GroupId;
                        userGroup1.UserId = au.UserId;

                        var exist = _context.UserGroups.FirstOrDefault(id=>(id.GroupId == au.GroupId) && (id.UserId == au.UserId));
                        if (exist == null)
                        {
                            _context.UserGroups.Add(userGroup1);
                        }

                    }
                }*/

                _context.SaveChanges();





                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened! " + Ex.Message);
            }
        }

        public static List<DateTime> getAllDates(int year, int month)
        {
            var ret = new List<DateTime>();
            for (int i = 1; i <= DateTime.DaysInMonth(year, month); i++)
            {
                ret.Add(new DateTime(year, month, i));
            }
            return ret;
        }

        public List<FunctionUnitChildren> saveAssetFunctionUnitsChildren(ProductionFlowAsset asset)
        {
            List<FunctionUnitChildren> fucTemp = new List<FunctionUnitChildren>();
            List<FunctionUnitChildren> orderdFucs = new List<FunctionUnitChildren>();

            List<FunctionUnit> functionUnits = _context.FunctionUnits.Where(id => (id.AssetId == asset.AssetId) && (id.ClientAssetNameId == asset.ClientAssetNameId)).ToList();
            functionUnits.OrderBy(id => id.FunctionUnitId);


            List<FunctionUnitChildren> functionUnitChildrens = _context.FunctionUnitChildrens.Where(id => (id.AssetId == 0) && (id.ClientAssetNameId == 0)).ToList();

            orderdFucs = functionUnitChildrens.OrderBy(id => id.FunctionUnitChildrenId).ToList();

            foreach (var fuc in orderdFucs)
            {
                FunctionUnitChildren functionUnitChildren = new FunctionUnitChildren();
                functionUnitChildren.AssetId = asset.AssetId;
                functionUnitChildren.ClientAssetNameId = asset.ClientAssetNameId;
                functionUnitChildren.Frequency = fuc.Frequency;
                functionUnitChildren.FunctionUnitChildrenName = fuc.FunctionUnitChildrenName;
                functionUnitChildren.FunctionChildrenBachgroundColor = fuc.FunctionChildrenBachgroundColor;
                functionUnitChildren.FunctionUnitId = fuc.FunctionUnitId;
                functionUnitChildren.FunctionChildrenColor = fuc.FunctionChildrenColor;
                functionUnitChildren.MeasurementUnit = fuc.MeasurementUnit;
                functionUnitChildren.FunctionUnitChildrenId = fuc.FunctionUnitChildrenId;
                fucTemp.Add(functionUnitChildren);
            }

            List<int> tempFUIds = new List<int>();

            foreach (var item in _context.FunctionUnits.Take(5).OrderByDescending(id => id.FunctionUnitId))
            {
                tempFUIds.Add(item.FunctionUnitId);
            }

            tempFUIds.Sort();

            List<int> holdings = new List<int>();
            List<int> holdings1 = new List<int>();

            holdings = tempFUIds;
            holdings1 = tempFUIds;

            int counter = 0;

            List<FunctionUnitChildren> fucTemp2 = new List<FunctionUnitChildren>();

            foreach (var item in holdings)
            {
                counter = counter + 1;

                foreach (var it in fucTemp.Where(id => id.FunctionUnitId == counter).ToList())
                {
                    FunctionUnitChildren functionUnitChildren1 = new FunctionUnitChildren();
                    functionUnitChildren1.AssetId = asset.AssetId;
                    functionUnitChildren1.ClientAssetNameId = asset.ClientAssetNameId;
                    functionUnitChildren1.Frequency = it.Frequency;
                    functionUnitChildren1.FunctionUnitChildrenName = it.FunctionUnitChildrenName;
                    functionUnitChildren1.FunctionChildrenBachgroundColor = it.FunctionChildrenBachgroundColor;
                    functionUnitChildren1.FunctionUnitId = holdings1[counter - 1];
                    functionUnitChildren1.FunctionChildrenColor = it.FunctionChildrenColor;
                    functionUnitChildren1.MeasurementUnit = it.MeasurementUnit;
                    fucTemp2.Add(functionUnitChildren1);
                }

            }

            _context.FunctionUnitChildrens.AddRange(fucTemp2);
            _context.SaveChanges();

            return fucTemp2;
        }

        public List<FunctionUnit> saveAssetFunctionUnits(ProductionFlowAsset asset)
        {

            List<FunctionUnit> functionUnits = _context.FunctionUnits.Where(id => id.AssetId == 0).ToList();
            List<FunctionUnit> fucTemp = new List<FunctionUnit>();

            foreach (FunctionUnit item in functionUnits)
            {
                FunctionUnit functionUnit = new FunctionUnit();
                functionUnit.AssetId = asset.AssetId;
                functionUnit.ClientAssetNameId = asset.ClientAssetNameId;
                functionUnit.FunctionUnitName = item.FunctionUnitName;
                fucTemp.Add(functionUnit);
            }

            _context.FunctionUnits.AddRange(fucTemp);
            _context.SaveChanges();

            return fucTemp;
        }

        public ProductionFlowAsset CreateAsset(ProductionFlowAsset model)
        {
            ProductionFlowAsset asset = new ProductionFlowAsset();
            asset.ClientAssetNameId = model.ClientAssetNameId;
            asset.DateStamp = DateTime.Now;
            asset.Reference = model.Reference;
            asset.TemplateId = model.TemplateId;
            asset.SiteName = model.SiteName;
            asset.AssetId = model.AssetId;
            asset.SinceDateProduction = DateTime.Now;

            _context.ProductionFlowAssets.Add(asset);
            _context.SaveChanges();

            return asset;
        }


        //[HttpPost("organization")]
        //public IActionResult PostOrganization([FromBody] Organization organization)
        //{
        //    if (organization == null)
        //    {
        //        return BadRequest("Something bad happened. Object is null");
        //    }

        //    try
        //    {
        //        Organization model = new Organization();
        //        model.ParentOrganizationId = organization.ParentOrganizationId;
        //        model.Name = organization.Name;
        //        model.DateStamp = DateTime.Now;
        //        model.Reference = organization.Reference;

        //        if (model.ParentOrganizationId == 0)
        //        {
        //            model.RootOrganizationId = 0;
        //        }

        //        if (organization.IsParentAddress)
        //        {
        //            if (model.ParentOrganizationId != 0)
        //            {
        //                model.ParentOrganizationId = organization.ParentOrganizationId;
        //            }
        //            else
        //            {
        //                model.ParentOrganizationId = 0;
        //            }

        //            Organization parentOrganization = _context.Organizations.FirstOrDefault(id => id.OrganizationId == organization.ParentOrganizationId);
        //            model.Address = parentOrganization.Address;
        //            model.Address2 = parentOrganization.Address2;
        //            model.City = parentOrganization.City;
        //            model.Zip = parentOrganization.Zip;
        //            model.CountryId = parentOrganization.CountryId;
        //            model.Code = organization.Code;
        //        }
        //        else
        //        {
        //            if (model.ParentOrganizationId != 0)
        //            {
        //                model.ParentOrganizationId = organization.ParentOrganizationId;
        //            }
        //            else
        //            {
        //                model.ParentOrganizationId = 0;
        //            }

        //            model.Address = organization.Address;
        //            model.Address2 = organization.Address2;
        //            model.City = organization.City;
        //            model.Zip = organization.Zip;
        //            model.CountryId = organization.CountryId;
        //            model.Code = organization.Code;
        //        }

        //        _context.Organizations.Add(model);
        //        _context.SaveChanges();




        //        var isRoot = true;
        //        Organization root = new Organization();

        //        if (model.ParentOrganizationId == 0)
        //        {
        //            model.RootOrganizationId = 0;
        //        }
        //        else
        //        {
        //            root = _context.Organizations.FirstOrDefault(id => (id.ParentOrganizationId == organization.ParentOrganizationId) && (id.Reference == organization.Reference));

        //            while (isRoot)
        //            {
        //                if (root.ParentOrganizationId == 0)
        //                    isRoot = false;
        //                else
        //                    root = _context.Organizations.FirstOrDefault(id => id.OrganizationId == root.ParentOrganizationId);
        //            }
        //            model.RootOrganizationId = root.OrganizationId;
        //        }



        //        if (model.ParentOrganizationId == 0)
        //        {
        //            model.Height = model.Height + 1;
        //        }
        //        else
        //        {
        //            model.Height = _context.Organizations.FirstOrDefault(id => id.OrganizationId == model.ParentOrganizationId).Height + 1;
        //        }

        //        _context.SaveChanges();

        //        return Ok();
        //    }
        //    catch (Exception Ex)
        //    {
        //        return BadRequest("Something bad happened! " + Ex.Message);
        //    }
        //}


        //[HttpPost("productiveUnit")]
        //public IActionResult PostProductiveUnit([FromBody] ProductiveUnit organization)
        //{
        //    if (organization == null)
        //    {
        //        return BadRequest("Something bad happened. Object is null");
        //    }

        //    try
        //    {
        //        ProductiveUnit model = new ProductiveUnit();
        //        model.OrganizationId = organization.OrganizationId;
        //        model.Name = organization.Name;
        //        model.DateStamp = DateTime.Now;
        //        model.Reference = organization.Reference;
        //        model.RootOrganizationId = _context.Organizations.FirstOrDefault(id => id.OrganizationId == organization.OrganizationId).RootOrganizationId;

        //        if (organization.ParentProductiveUnitId != 0)
        //        {
        //            model.ParentProductiveUnitId = organization.ParentProductiveUnitId;
        //            model.Height = _context.ProductiveUnits.Where(o => o.ProductiveUnitId == organization.ParentProductiveUnitId && o.Reference.Equals(organization.Reference)).Max(h => h.Height) + 1;
        //        }
        //        else
        //        {
        //            model.ParentProductiveUnitId = 0;
        //            model.Height = _context.Organizations.Where(o => o.OrganizationId == organization.OrganizationId && o.Reference.Equals(organization.Reference)).Max(h => h.Height) + 1;
        //        }

        //        if (organization.IsParentAddress)
        //        {
        //            ProductiveUnit parentOrganization = _context.ProductiveUnits.FirstOrDefault(id => id.ProductiveUnitId == organization.ParentProductiveUnitId);
        //            model.Address = parentOrganization.Address;
        //            model.Address2 = parentOrganization.Address2;
        //            model.City = parentOrganization.City;
        //            model.Zip = parentOrganization.Zip;
        //            model.CountryId = parentOrganization.CountryId;
        //            model.Code = organization.Code;
        //            model.OrganizationId = organization.OrganizationId;
                    
        //        }
        //        else
        //        {
                    
        //            model.OrganizationId = organization.OrganizationId;
        //            model.Address = organization.Address;
        //            model.Address2 = organization.Address2;
        //            model.City = organization.City;
        //            model.Zip = organization.Zip;
        //            model.CountryId = organization.CountryId;
        //            model.Code = organization.Code;
        //        }

        //        _context.ProductiveUnits.Add(model);
        //        _context.SaveChanges();

        //        return Ok();
        //    }
        //    catch (Exception Ex)
        //    {
        //        return BadRequest("Something bad happened! " + Ex.Message);
        //    }
        //}


        //[HttpPost("equipment")]
        //public IActionResult PostEquipment([FromBody] Equipment equipment)
        //{
        //    if (equipment == null)
        //    {
        //        return BadRequest("Something bad happened. Object is null");
        //    }

        //    try
        //    {
        //        Equipment model = new Equipment();
        //        model.ProductiveUnitId = equipment.ProductiveUnitId;
        //        model.Name = equipment.Name;
        //        model.DateStamp = DateTime.Now;
        //        model.Reference = equipment.Reference;
        //        model.RootOrganizationId = _context.ProductiveUnits.FirstOrDefault(id => id.ProductiveUnitId == equipment.ProductiveUnitId).RootOrganizationId;

        //        if (equipment.ParentEquipmentId != 0)
        //        {
        //            model.ParentEquipmentId = equipment.ParentEquipmentId;
        //            model.Height = _context.Equipments.Where(o => o.EquipmentId == equipment.ParentEquipmentId && o.Reference.Equals(equipment.Reference)).Max(h => h.Height) + 1;

        //        }
        //        else
        //        {
        //            model.ParentEquipmentId = 0;
        //            model.Height = _context.ProductiveUnits.Where(o => o.ProductiveUnitId == equipment.ProductiveUnitId && o.Reference.Equals(equipment.Reference)).Max(h => h.Height) + 1;
        //        }

        //        if (equipment.IsParentAddress)
        //        {

        //            if (equipment.ParentEquipmentId == 0)
        //            {
        //                ProductiveUnit productiveUnit = _context.ProductiveUnits.FirstOrDefault(id => id.ProductiveUnitId == equipment.ProductiveUnitId);
        //                model.Address = productiveUnit.Address;
        //                model.Address2 = productiveUnit.Address2;
        //                model.City = productiveUnit.City;
        //                model.Zip = productiveUnit.Zip;
        //                model.CountryId = productiveUnit.CountryId;
        //            }
        //            else
        //            {
        //                Equipment equip = _context.Equipments.FirstOrDefault(id => id.EquipmentId == equipment.ParentEquipmentId);
        //                model.Address = equip.Address;
        //                model.Address2 = equip.Address2;
        //                model.City = equip.City;
        //                model.Zip = equip.Zip;
        //                model.CountryId = equip.CountryId;
        //            }

        //            model.Code = equipment.Code;
        //            model.ProductiveUnitId = equipment.ProductiveUnitId;
        //        }
        //        else
        //        {
        //            model.ProductiveUnitId = equipment.ProductiveUnitId;
        //            model.Address = equipment.Address;
        //            model.Address2 = equipment.Address2;
        //            model.City = equipment.City;
        //            model.Zip = equipment.Zip;
        //            model.CountryId = equipment.CountryId;
        //            model.Code = equipment.Code;
        //        }

        //        _context.Equipments.Add(model);
        //        _context.SaveChanges();

        //        return Ok();
        //    }
        //    catch (Exception Ex)
        //    {
        //        return BadRequest("Something bad happened! " + Ex.Message);
        //    }
        //}

        [HttpGet("tree")]
        public IActionResult GetTree(string reference)
        {
            if (reference == "")
            {
                return BadRequest("Something bad happened, Log out and Log in and try again");
            }

            List<Tree1> data = new List<Tree1>();
            Tree1 rootTree= null;

            List<OrganizationVM> organizations = new OrganizationService(_context).GetOrganizationVMs(reference);

            foreach (var org in organizations.Where(a => a.Height == 1))
            {
                rootTree  = new Tree1();
                rootTree.Name = string.Format("{0}-{1}", org.Code, org.Name);
                rootTree.Children = new List<Tree1>();
                rootTree.NodeId = org.OrganizationId;
                rootTree.ParentNodeId = org.ParentOrganizationId;
                rootTree.NodeType = org.NodeType;
                rootTree.Checked = false;

                data.Add(rootTree);

                var allChildrens = organizations.Where(a => a.RootOrganizationId == org.OrganizationId).OrderBy(p => p.Height).OrderBy(p => p.ParentOrganizationId).ToList();
                
                if (allChildrens.Count > 0)
                {
                    var maxHeight = allChildrens.Max(a => a.Height);
                    
                    Tree1 currntParent = rootTree;
                    List<Tree1> parents = new List<Tree1>();

                    for (int i = 2; i <= maxHeight; i++)
                    {                         
                        Tree1 treeBranch = null;
                        Tree1 currentParent = null;

                        var t = allChildrens.OrderBy(h => h.Height).ToList();

                        var parentIds = t.Where(a=> a.Height == i).Select(a => a.ParentOrganizationId).Distinct().ToList();

                        foreach (var id in parentIds)
                        {
                            foreach (var child in t.Where(a => a.Height == i && a.ParentOrganizationId == id))
                            {
                                treeBranch = new Tree1();
                                treeBranch.NodeId = child.OrganizationId;
                                treeBranch.ParentNodeId = child.ParentOrganizationId;
                                treeBranch.NodeType = child.NodeType;
                                treeBranch.Name = string.Format("{0}-{1}", child.Code, child.Name);
                                treeBranch.Children = new List<Tree1>();
                                treeBranch.Checked = false;

                                parents.Add(treeBranch);

                                if (child.ParentOrganizationId != rootTree.NodeId)
                                    currentParent = parents.FirstOrDefault(a => a.NodeId == child.ParentOrganizationId);

                                if (currentParent != null)
                                {
                                    currentParent.Children.Add(treeBranch);
                                }
                                else
                                    rootTree.Children.Add(treeBranch);
                            }
                            if(currentParent != null)
                            {
                                rootTree = currentParent;
                            }
                            else
                            rootTree = treeBranch;
                        }

                    }

                }
                            
            }

            return Ok(data);
        }

        [HttpGet("assetNodesTable")]
        public IActionResult GetAssetNodesTable(string reference)
        {
            if (reference == "")
            {
                return BadRequest("Something bad happened, make sure you are logged in.");
            }

            try
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

                    groups.Add(_context.Groups.FirstOrDefault(id=>id.GroupId == gId));
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



                //List <AssetNode> assetNodes = _context.AssetNodes.Where(id => (id.Reference == reference) && (id.ParentAssetNodeId != 0)).ToList();

                var assetNodesSrted = assetNodes.OrderBy(id => id.RootAssetNodeId).ToList();

                var assetNodesVM = assetNodesSrted.Select(result => new
                {
                    AssetNodeId = result.AssetNodeId,
                    AssetNodeType = _context.AssetNodeTypes.FirstOrDefault(id=>id.AssetNodeTypeId == result.AssetNodeTypeId).AssetNodeTypeName,
                    CountryId = result.CountryId,
                    Name = result.Name,
                    Code = result.Code,
                    ParentId = result.ParentAssetNodeId == 0 ? result.AssetNodeId : result.ParentAssetNodeId,
                    Parent = result.ParentAssetNodeId == 0 ? _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == result.AssetNodeId).Name :_context.AssetNodes.FirstOrDefault(id=> id.AssetNodeId == result.ParentAssetNodeId ).Name,
                    Root = result.RootAssetNodeId == 0 ? _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == result.AssetNodeId).Name : _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == result.RootAssetNodeId).Name,
                    Address = result.Address,
                    Address2 = result.Address2,
                    City = result.City,
                    Zip = result.Zip,
                    AssetNodeTypeId = result.AssetNodeTypeId,
                    IsProductionFlow = result.isProductionFlow
                });

               
                return Ok(assetNodesVM);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened, " + Ex.Message);
            }
        
        }

        [HttpPost("deleteAssetNode")]
        public IActionResult DeleteAssetNode([FromBody] DeleteAssetNode model)
        {
            if (model.AssetNodeId == 0)
            {
                return BadRequest("Something bad happened!. Try again later.");
            }

            try
            {
                AssetNode assetNode = _context.AssetNodes.FirstOrDefault(id => (id.AssetNodeId == model.AssetNodeId) && (id.Reference == model.Reference));

                List<AssetNode> assetNodes = _context.AssetNodes.Where(id => (id.ParentAssetNodeId == model.AssetNodeId) && (id.Reference == model.Reference)).ToList();

                if (assetNodes.Count > 1)
                {
                    return BadRequest("You can not delete nodes with dependencies, start by deleting the branches.");
                }

                List<Reading> readings = _context.Readings.Where(id => id.AssetId == model.AssetNodeId).ToList();

                if (readings.Count > 0)
                {
                    _context.Readings.RemoveRange(readings);
                    var assetUsers = _context.AssetUsers.Where(id => id.AssetNodeId == model.AssetNodeId).ToList();
                    _context.AssetUsers.RemoveRange(assetUsers);

                    var prodFlowAssets = _context.ProductionFlowAssets.Where(id => (id.AssetId == model.AssetNodeId) || (id.ClientAssetNameId == model.AssetNodeId)).ToList();
                    _context.ProductionFlowAssets.RemoveRange(prodFlowAssets);


                    var prodFlowClientAssetName = _context.ClientAssetNames.Where(id => id.ClientAssetNameId == model.AssetNodeId).ToList();
                    _context.ClientAssetNames.RemoveRange(prodFlowClientAssetName);

                    _context.SaveChanges();

                }

                int GroupId = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == model.AssetNodeId).GroupId;

                //Remove corresponding groups
                List<Group> groups = _context.Groups.Where(id => id.GroupId == GroupId).ToList();
                _context.Groups.RemoveRange(groups);
                _context.SaveChanges();

                foreach (var item in groups)
                {
                    //remove corresponding usergrop
                    List<UserGroup> userGroups = _context.UserGroups.Where(id => id.GroupId == item.GroupId).ToList();
                    _context.UserGroups.RemoveRange(userGroups);
                    _context.SaveChanges();
                }


                _context.AssetNodes.Remove(assetNode);

                _context.SaveChanges();

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened! " + Ex.Message);
            }

        }

        [HttpPost("addUserAssetNode")]
        public IActionResult AddUserAssetNode([FromBody] AssetUser model)
        {
            if (model.AssetNodeId == 0 || model.UserId == "")
            {
                return BadRequest("Something bad happened!. Try again later.");
            }

            try
            {
                AssetNode assetNode = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == model.AssetNodeId);

                AssetUser lAssetUser = _context.AssetUsers.FirstOrDefault(id => (id.UserId == model.UserId) && (id.AssetNodeId == model.AssetNodeId));

                if (lAssetUser != null)
                {
                    return BadRequest("Something bad happened, Make sure you are not trying to duplicate. Search the table.");
                }

                AssetUser assetUser = new AssetUser();
                assetUser.UserId = model.UserId;
                assetUser.AssetNodeId = model.AssetNodeId;
                assetUser.AssetNodeTypeId = assetNode.AssetNodeTypeId;
                assetUser.DateStamp = DateTime.Now;
                assetUser.IsAssetAdmin = model.IsAssetAdmin;
                assetUser.Reference = model.Reference;

                _context.AssetUsers.Add(assetUser);
                _context.SaveChanges();

                //Check if admin is added as an asset user for this asset node, if not: add

                var isAdminAddedAsAssetNodeUser = _context.AssetUsers.FirstOrDefault(id => (id.UserId == model.Reference) && (id.AssetNodeId == model.AssetNodeId));

                if (isAdminAddedAsAssetNodeUser == null)
                {
                    AssetUser assetUser1 = new AssetUser();
                    assetUser1.UserId = model.Reference;
                    assetUser1.AssetNodeId = model.AssetNodeId;
                    assetUser1.AssetNodeTypeId = assetNode.AssetNodeTypeId;
                    assetUser1.DateStamp = DateTime.Now;
                    assetUser1.IsAssetAdmin = model.IsAssetAdmin;
                    assetUser1.Reference = model.Reference;

                    _context.AssetUsers.Add(assetUser1);
                    _context.SaveChanges();
                }

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened! " + Ex.Message);
            }

        }

        [HttpGet("GetMyAssetNodes")]
        public IActionResult GetMyAssetNode(string reference)
        {
            if (reference == "")
            {
                return BadRequest("Something bad happened. Make sure you are correctly logged on.");
            }

            try
            {
                List<AssetUser> assetUsers = _context.AssetUsers.Where(id => id.UserId == reference).ToList();

                var assetNodeUsers = assetUsers.Select(result => new
                {
                    AssetUserId = result.AssetUserId,
                    AssetNodeId = result.AssetNodeId,
                    AssetNodeTypeId = result.AssetNodeTypeId,
                    AssetNodeAccessLeve = result.IsAssetAdmin,
                    AssetNodeTypeName = _context.AssetNodeTypes.FirstOrDefault(id => id.AssetNodeTypeId == result.AssetNodeTypeId).AssetNodeTypeName,
                    AssetNode = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == result.AssetNodeId).Name,
                    UserEmail = _context.Users.FirstOrDefault(id => id.Id == result.UserId).Email,
                    UserNames = _context.Users.FirstOrDefault(id => id.Id == result.UserId).FirstName + " " + _context.Users.FirstOrDefault(id => id.Id == result.UserId).LastName,
                    Date = result.DateStamp,
                    Reference = _context.Users.FirstOrDefault(id => id.Id == result.Reference).FirstName + " " + _context.Users.FirstOrDefault(id => id.Id == result.Reference).LastName,
                });

                return Ok(assetNodeUsers);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        [HttpGet("GetUserAssetNodes")]
        public IActionResult AddUserAssetNode(string reference)
        {
            if (reference == "")
            {
                return BadRequest("Something bad happened. Make sure you are correctly logged on.");
            }

            try
            {
                List<AssetUser> assetUsers = _context.AssetUsers.Where(id => id.Reference == reference).ToList();

                var assetNodeUsers = assetUsers.Select(result => new
                {
                    AssetUserId = result.AssetUserId,
                    AssetNodeId = result.AssetNodeId,
                    AssetNodeTypeId = result.AssetNodeTypeId,
                    AssetNodeAccessLeve = result.IsAssetAdmin,
                    AssetNodeTypeName = _context.AssetNodeTypes.FirstOrDefault(id=>id.AssetNodeTypeId==result.AssetNodeTypeId).AssetNodeTypeName,
                    AssetNode = _context.AssetNodes.FirstOrDefault(id=>id.AssetNodeId == result.AssetNodeId).Name,
                    UserEmail = _context.Users.FirstOrDefault(id=>id.Id == result.UserId).Email,
                    UserNames = _context.Users.FirstOrDefault(id => id.Id == result.UserId).FirstName + " " + _context.Users.FirstOrDefault(id => id.Id == result.UserId).LastName,
                    Date = result.DateStamp,
                    Reference = _context.Users.FirstOrDefault(id => id.Id == result.Reference).FirstName + " " + _context.Users.FirstOrDefault(id => id.Id == result.Reference).LastName,
                });

                return Ok(assetNodeUsers);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }


        [HttpGet("assetNodeTree")]
        public IActionResult GetAssetNodeTree(string reference)
        {
            if (reference == "")
            {
                return BadRequest("Something bad happened, Log out and Log in and try again");
            }

            List<Tree1> data = new List<Tree1>();
            Tree1 rootTree = null;

            List<AssetNodeVM> assetNodes = new AssetNodeService(_context).GetAssetNodeVMs(reference);

            AssetNodeVM assetNodeVM = assetNodes.FirstOrDefault(id => id.Height == 1);

            if (assetNodeVM == null)
            {
                if (assetNodes.Count > 0)
                {
                    int firstHeight = assetNodes[0].Height - 1;
                    List<AssetNodeVM> assetNodesTemp = new List<AssetNodeVM>();
                    int flyRoot = assetNodes[0].AssetNodeId;
                    foreach (var an in assetNodes)
                    {
                        AssetNodeVM assetNodeVM1 = new AssetNodeVM();
                        assetNodeVM1 = an;
                        assetNodeVM1.Height = assetNodeVM1.Height - firstHeight;
                        assetNodeVM1.RootAssetNodeId = flyRoot;
                        assetNodesTemp.Add(assetNodeVM1);
                    }

                    assetNodes = new List<AssetNodeVM>();
                    assetNodes = assetNodesTemp;
                }

            }
            


            foreach (var asd in assetNodes.Where(a => a.Height == 1))
            {
                rootTree = new Tree1();
                rootTree.Name = string.Format("{0}-{1}", asd.Code, asd.Name);
                rootTree.Children = new List<Tree1>();
                rootTree.NodeId = asd.AssetNodeId;
                rootTree.ParentNodeId = asd.ParentAssetNodeId;
                rootTree.NodeType = asd.NodeType;
                rootTree.Checked = false;

                data.Add(rootTree);

                var allChildrens = assetNodes.Where(a => a.RootAssetNodeId == asd.AssetNodeId)
                    .OrderBy(p => p.Height).OrderBy(p => p.ParentAssetNodeId).ToList();

                if (allChildrens.Count > 0)
                {
                    var maxHeight = allChildrens.Max(a => a.Height);

                    Tree1 currntParent = rootTree;
                    List<Tree1> parents = new List<Tree1>();

                    for (int i = 2; i <= maxHeight; i++)
                    {                     
                        Tree1 treeBranch = null;
                        Tree1 currentParent = null;

                        var t = allChildrens.OrderBy(h => h.Height).ToList();

                        var parentIds = t.Where(a => a.Height == i).Select(a => a.ParentAssetNodeId).Distinct().ToList();

                        foreach (var id in parentIds)
                        {
                            foreach (var child in t.Where(a => a.Height == i && a.ParentAssetNodeId == id))
                            {
                                treeBranch = new Tree1();
                                treeBranch.NodeId = child.AssetNodeId;
                                treeBranch.ParentNodeId = child.ParentAssetNodeId;
                                treeBranch.NodeType = child.NodeType;
                                treeBranch.Name = string.Format("{0}-{1}", child.Code, child.Name);
                                treeBranch.Children = new List<Tree1>();
                                treeBranch.Checked = false;

                                parents.Add(treeBranch);

                                if (child.ParentAssetNodeId != rootTree.NodeId)
                                    currentParent = parents.FirstOrDefault(a => a.NodeId == child.ParentAssetNodeId);

                                if (currentParent != null)
                                {
                                    currentParent.Children.Add(treeBranch);
                                }
                                else
                                    rootTree.Children.Add(treeBranch);
                            }
                            if (currentParent != null)
                            {
                                rootTree = currentParent;
                            }
                            else
                                rootTree = treeBranch;
                        }

                    }

                }

            }

            return Ok(data);
        }

        [HttpGet("addProductionFlow")]
        public IActionResult addProductionFlow(int assetNodeId)
        {
            if (assetNodeId == 0)
            {
                return BadRequest("Something bad happened. Asset Node is selected.");
            }

            try
            {
                AssetNode assetNode = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == assetNodeId);
                assetNode.isProductionFlow = true;
                _context.AssetNodes.Update(assetNode);
                _context.SaveChanges();


                ClientAsset clientAssetName = new ClientAsset();
                clientAssetName.AssetName = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == assetNode.ParentAssetNodeId).Name;
                clientAssetName.DateStamp = DateTime.Now;
                clientAssetName.Reference = assetNode.Reference;
                clientAssetName.ClientName = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == assetNode.RootAssetNodeId).Name;
                clientAssetName.ClientAssetNameId = assetNode.ParentAssetNodeId;
                clientAssetName.AssetNodeId = assetNodeId;

                _context.ClientAssetNames.Add(clientAssetName);
                _context.SaveChanges();

                /*//Add Asset, from a production's flow perspective
                BinmakBackEnd.Areas.ProductionFlow.Entities.ProductionFlowAsset productionFlowAsset = new Areas.ProductionFlow.Entities.ProductionFlowAsset();
                productionFlowAsset.AssetId = assetNode.AssetNodeId;
                productionFlowAsset.ClientAssetNameId = assetNode.ParentAssetNodeId;
                productionFlowAsset.SiteName = assetNode.Name;
                productionFlowAsset.Reference = assetNode.Reference;
                productionFlowAsset.TemplateId = 1;
                productionFlowAsset.SinceDateProduction = DateTime.Now;
                productionFlowAsset.DateStamp = DateTime.Now;
                _context.ProductionFlowAssets.Add(productionFlowAsset);
                _context.SaveChanges();

                List<FunctionUnit> functionUnits = saveAssetFunctionUnits(productionFlowAsset);
                List<FunctionUnitChildren> functionUnitsChildren = saveAssetFunctionUnitsChildren(productionFlowAsset);

                List<DateTime> dates = getAllDates(productionFlowAsset.SinceDateProduction.Year, productionFlowAsset.SinceDateProduction.Month);
                List<Reading> readings = new List<Reading>();

                foreach (var date in dates)
                {
                    Reading reading = new Reading();
                    reading.DateProduction = date;
                    reading.AssetId = productionFlowAsset.AssetId;
                    reading.Reference = productionFlowAsset.Reference;

                    readings.Add(reading);

                }

                _context.Readings.AddRange(readings);*/
                _context.SaveChanges();

                return Ok();

            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened " + Ex.Message);
            }
        }

        [HttpGet("removeProductionFlow")]
        public IActionResult removeProductionFlow(int assetNodeId)
        {
            if (assetNodeId == 0)
            {
                return BadRequest("Something bad happened. Asset Node is selected.");
            }

            try
            {
                AssetNode assetNode = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == assetNodeId);
                assetNode.isProductionFlow = false;
                _context.AssetNodes.Update(assetNode);
                _context.SaveChanges();
                
                /*ClientAsset clientAsset = _context.ClientAssetNames.FirstOrDefault(id => id.ClientAssetNameId == assetNode.ParentAssetNodeId);
                int clientAssetId = clientAsset.ClientAssetNameId;
                _context.ClientAssetNames.Remove(clientAsset);
                _context.SaveChanges();


                List<ProductionFlowAsset> productionFlowAssets = _context.ProductionFlowAssets.Where(id => id.ClientAssetNameId == clientAssetId).ToList();

                foreach (var item in productionFlowAssets)
                {
                    List<Reading> readings = _context.Readings.Where(id => id.AssetId == item.AssetId).ToList();
                    _context.Readings.RemoveRange(readings);
                }

                _context.ProductionFlowAssets.RemoveRange(productionFlowAssets);
                _context.SaveChanges();*/

                return Ok();

            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened " + Ex.Message);
            }
        }

    }
}
