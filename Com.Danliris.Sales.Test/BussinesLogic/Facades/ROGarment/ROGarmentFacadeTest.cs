﻿using Com.Danliris.Sales.Test.BussinesLogic.DataUtils.Garment.GarmentMerchandiser;
using Com.Danliris.Sales.Test.BussinesLogic.DataUtils.GarmentPreSalesContractDataUtils;
using Com.Danliris.Sales.Test.BussinesLogic.DataUtils.RoGarmentDataUtils;
using Com.Danliris.Sales.Test.BussinesLogic.Utils;
using Com.Danliris.Service.Sales.Lib;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Facades.CostCalculationGarments;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Facades.Garment;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Facades.GarmentPreSalesContractFacades;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Facades.ROGarment;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Interface;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Interface.CostCalculationGarmentLogic;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Logic.CostCalculationGarments;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Logic.Garment;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Logic.GarmentPreSalesContractLogics;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Logic.ROGarmentLogics;
using Com.Danliris.Service.Sales.Lib.Helpers;
using Com.Danliris.Service.Sales.Lib.Models.CostCalculationGarments;
using Com.Danliris.Service.Sales.Lib.Models.ROGarments;
using Com.Danliris.Service.Sales.Lib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.Danliris.Sales.Test.BussinesLogic.Facades.ROGarment
{
    public class ROGarmentFacadeTest 
    {
        private const string ENTITY = "ROGarment";
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return string.Concat(sf.GetMethod().Name, "_", ENTITY);
        }
        //public ROGarmentFacadeTest() : base(ENTITY)
        //{
        //}

        protected SalesDbContext DbContext(string testName)
        {
            DbContextOptionsBuilder<SalesDbContext> optionsBuilder = new DbContextOptionsBuilder<SalesDbContext>();
            optionsBuilder
                .UseInMemoryDatabase(testName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            SalesDbContext dbContext = new SalesDbContext(optionsBuilder.Options);

            return dbContext;
        }



        protected ROGarmentDataUtil DataUtil(ROGarmentFacade facade, SalesDbContext dbContext = null)
        {
            var serviceProvider = GetServiceProviderMock(dbContext).Object;

            GarmentPreSalesContractFacade garmentPreSalesContractFacade = new GarmentPreSalesContractFacade(serviceProvider, dbContext);
            GarmentPreSalesContractDataUtil garmentPreSalesContractDataUtil = new GarmentPreSalesContractDataUtil(garmentPreSalesContractFacade);
            CostCalculationGarmentFacade costCalculationGarmentFacade = new CostCalculationGarmentFacade(serviceProvider, dbContext);
            CostCalculationGarmentDataUtil costCalculationGarmentDataUtil = new CostCalculationGarmentDataUtil(costCalculationGarmentFacade, garmentPreSalesContractDataUtil);

            ROGarmentDataUtil roGarmentDataUtil = new ROGarmentDataUtil(facade, costCalculationGarmentDataUtil);
            return roGarmentDataUtil;
        }

        protected Mock<IServiceProvider> GetServiceProviderMock(SalesDbContext dbContext)
        {
            var serviceProviderMock = new Mock<IServiceProvider>();

            IIdentityService identityService = new IdentityService { Username = "Username" };

           

            //ROGarmentLogic roGarmentLogic = new ROGarmentLogic(serviceProviderMock.Object, identityService, dbContext);
            
            

            CostCalculationGarmentMaterialLogic costCalculationGarmentMaterialLogic = new CostCalculationGarmentMaterialLogic(serviceProviderMock.Object, identityService, dbContext);
            

            GarmentPreSalesContractLogic garmentPreSalesContractLogic = new GarmentPreSalesContractLogic(identityService, dbContext);

            var azureImageFacadeMock = new Mock<IAzureImageFacade>();
            azureImageFacadeMock
                .Setup(s => s.DownloadImage(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("[\"test\"]");
            azureImageFacadeMock
                .Setup(s => s.UploadMultipleImage(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<List<string>>(), It.IsAny<string>()))
                .ReturnsAsync("[\"test\"]");
            azureImageFacadeMock
                .Setup(s => s.RemoveMultipleImage(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(0));

            serviceProviderMock
                .Setup(x => x.GetService(typeof(IIdentityService)))
                .Returns(identityService);
            serviceProviderMock
                .Setup(x => x.GetService(typeof(CostCalculationGarmentMaterialLogic)))
                .Returns(costCalculationGarmentMaterialLogic);

            CostCalculationGarmentLogic costCalculationGarmentLogic = new CostCalculationGarmentLogic(costCalculationGarmentMaterialLogic, serviceProviderMock.Object, identityService, dbContext);
            serviceProviderMock
                .Setup(x => x.GetService(typeof(CostCalculationGarmentLogic)))
                .Returns(costCalculationGarmentLogic);
            CostCalculationGarmentFacade costCalculationGarmentFacade = new CostCalculationGarmentFacade(serviceProviderMock.Object, dbContext);
            serviceProviderMock
                .Setup(x => x.GetService(typeof(ICostCalculationGarment)))
                .Returns(costCalculationGarmentFacade);
            serviceProviderMock
                .Setup(x => x.GetService(typeof(GarmentPreSalesContractLogic)))
                .Returns(garmentPreSalesContractLogic);
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IAzureImageFacade)))
                .Returns(azureImageFacadeMock.Object);

            var azureDocumentFacadeMock = new Mock<IAzureDocumentFacade>();
            azureDocumentFacadeMock
                .Setup(s => s.DownloadMultipleFiles(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<string> { "[\"test\"]" });
            azureDocumentFacadeMock
                .Setup(s => s.UploadMultipleFile(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("[\"test\"]");
            azureDocumentFacadeMock
                .Setup(s => s.RemoveMultipleFile(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(0));

            serviceProviderMock
                .Setup(x => x.GetService(typeof(IAzureDocumentFacade)))
                .Returns(azureDocumentFacadeMock.Object);

            ROGarmentSizeBreakdownDetailLogic roGarmentSizeBreakdownDetailLogic = new ROGarmentSizeBreakdownDetailLogic(serviceProviderMock.Object, identityService, dbContext);
            serviceProviderMock
                .Setup(x => x.GetService(typeof(ROGarmentSizeBreakdownDetailLogic)))
                .Returns(roGarmentSizeBreakdownDetailLogic);

            ROGarmentSizeBreakdownLogic roGarmentSizeBreakdownLogic = new ROGarmentSizeBreakdownLogic(serviceProviderMock.Object, identityService, dbContext);
            serviceProviderMock
               .Setup(x => x.GetService(typeof(ROGarmentSizeBreakdownLogic)))
               .Returns(roGarmentSizeBreakdownLogic);

            ROGarmentLogic roGarmentLogic = new ROGarmentLogic(serviceProviderMock.Object, identityService, dbContext);
            serviceProviderMock
                .Setup(x => x.GetService(typeof(ROGarmentLogic)))
                .Returns(roGarmentLogic);
            //var costCalculationMock = new Mock<ICostCalculationGarment>();
            //costCalculationMock
            //    .Setup(s => s.ReadByIdAsync(It.IsAny<int>()));

            return serviceProviderMock;
        }

        [Fact]
        public virtual async void Create_Success()
        {
            var dbContext = DbContext(GetCurrentMethod());
            var serviceProvider = GetServiceProviderMock(dbContext).Object;

            ROGarmentFacade facade = Activator.CreateInstance(typeof(ROGarmentFacade), serviceProvider, dbContext) as ROGarmentFacade;

            var data = await DataUtil(facade, dbContext).GetNewData();

            var response = await facade.CreateAsync(data);

            Assert.NotEqual(response, 0);
        }

        [Fact]
        public virtual async void Get_All_Success()
        {
            var dbContext = DbContext(GetCurrentMethod());
            var serviceProvider = GetServiceProviderMock(dbContext).Object;

            ROGarmentFacade facade = Activator.CreateInstance(typeof(ROGarmentFacade), serviceProvider, dbContext) as ROGarmentFacade;

            var data = await DataUtil(facade, dbContext).GetTestData();

            var Response = facade.Read(1, 25, "{}", new List<string>(), "", "{}");

            Assert.NotEqual(Response.Data.Count, 0);
        }

        [Fact]
        public virtual async void Get_By_Id_Success()
        {
            var dbContext = DbContext(GetCurrentMethod());
            var serviceProvider = GetServiceProviderMock(dbContext).Object;

            ROGarmentFacade facade = Activator.CreateInstance(typeof(ROGarmentFacade), serviceProvider, dbContext) as ROGarmentFacade;

            var data = await DataUtil(facade, dbContext).GetTestData();

            var Response = facade.ReadByIdAsync((int)data.Id);

            Assert.NotEqual(Response.Id, 0);
        }

        [Fact]
        public virtual async void Delete_Success()
        {
            var dbContext = DbContext(GetCurrentMethod());
            var serviceProvider = GetServiceProviderMock(dbContext).Object;

            ROGarmentFacade facade = Activator.CreateInstance(typeof(ROGarmentFacade), serviceProvider, dbContext) as ROGarmentFacade;
            var data = await DataUtil(facade, dbContext).GetTestData();

            var Response = await facade.DeleteAsync((int)data.Id);
            Assert.NotEqual(Response, 0);
        }


        //[Fact]
        //public override async void Update_Success()
        //{
        //    var dbContext = DbContext(GetCurrentMethod());
        //    var serviceProvider = GetServiceProviderMock(dbContext).Object;

        //    ROGarmentFacade facade = new ROGarmentFacade(serviceProvider, dbContext);

        //    var data = await DataUtil(facade, dbContext).GetTestData();

        //    //var dataTemp = new RO_Garment
        //    //{
        //    //    Id = data.Id,
        //    //    CostCalculationGarmentId = data.CostCalculationGarmentId,
        //    //    CostCalculationGarment = data.CostCalculationGarment,
        //    //    Code = data.Code,
        //    //    Instruction = data.Instruction,
        //    //    RO_Garment_SizeBreakdowns = new List<RO_Garment_SizeBreakdown>(),
        //    //    Total = data.Total,
        //    //    ImagesFile = data.ImagesFile,
        //    //    ImagesName = data.ImagesName,
        //    //    ImagesPath = data.ImagesPath,
        //    //};

        //    //foreach (var item in data.RO_Garment_SizeBreakdowns)
        //    //{
        //    //    var itemTemp = new RO_Garment_SizeBreakdown
        //    //    {
        //    //        Id = item.Id,
        //    //        RO_GarmentId = item.RO_GarmentId,
        //    //        Code = item.Code,
        //    //        ColorId = item.ColorId,
        //    //        ColorName = item.ColorName,
        //    //        Total = item.Total,
        //    //        RO_Garment_SizeBreakdown_Details = new List<RO_Garment_SizeBreakdown_Detail>()
        //    //    };

        //    //    foreach (var detail in item.RO_Garment_SizeBreakdown_Details)
        //    //    {
        //    //        itemTemp.RO_Garment_SizeBreakdown_Details.Add(new RO_Garment_SizeBreakdown_Detail
        //    //        {
        //    //            Id = detail.Id,
        //    //            RO_Garment_SizeBreakdownId = detail.RO_Garment_SizeBreakdownId,
        //    //            Code = detail.Code,
        //    //            Size = detail.Size,
        //    //            Information = detail.Information,
        //    //            Quantity = detail.Quantity,
        //    //        });
        //    //    }

        //    //    dataTemp.RO_Garment_SizeBreakdowns.Add(itemTemp);
        //    //}

        //    var response = await facade.UpdateAsync((int)data.Id, data);

        //    Assert.NotEqual(response, 0);
        //}

        [Fact]
        public async Task PostRO_Success()
        {
            var dbContext = DbContext(GetCurrentMethod());
            var serviceProvider = GetServiceProviderMock(dbContext).Object;

            ROGarmentFacade facade = new ROGarmentFacade(serviceProvider, dbContext);
            var data = await DataUtil(facade, dbContext).GetTestData();

            var Response = await facade.PostRO(new List<long> { data.Id });
            Assert.NotEqual(Response, 0);

            var ResultData = await facade.ReadByIdAsync((int)data.Id);
            Assert.Equal(ResultData.IsPosted, true);
        }

        [Fact]
        public async Task UnpostRO_Success()
        {
            var dbContext = DbContext(GetCurrentMethod());
            var serviceProvider = GetServiceProviderMock(dbContext).Object;

            ROGarmentFacade facade = new ROGarmentFacade(serviceProvider, dbContext);
            var data = await DataUtil(facade, dbContext).GetTestData();

            var Response = await facade.UnpostRO(data.Id);
            Assert.NotEqual(Response, 0);

            var ResultData = await facade.ReadByIdAsync((int)data.Id);
            Assert.Equal(ResultData.IsPosted, false);
        }

        [Fact]
        public async Task UnpostRO_Error()
        {
            var dbContext = DbContext(GetCurrentMethod());
            var serviceProvider = GetServiceProviderMock(dbContext).Object;

            ROGarmentFacade facade = new ROGarmentFacade(serviceProvider, dbContext);

            await Assert.ThrowsAnyAsync<Exception>(async () => await facade.UnpostRO(0));
        }

    }
}