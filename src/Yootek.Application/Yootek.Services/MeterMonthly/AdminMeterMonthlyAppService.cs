﻿using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Linq.Extensions;
using Yootek.Authorization.Users;
using Yootek.Organizations;
using Yootek.Authorization;
using Yootek.QueriesExtension;
using Microsoft.AspNetCore.Http;
using System.IO;
using OfficeOpenXml;
using System.Globalization;
using Yootek.Common.Enum;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Bibliography;
using YOOTEK.EntityDb.Citizen;
using Yootek.Services.Dto;

namespace Yootek.Services
{
    public interface IAdminMeterMonthlyAppService : IApplicationService
    {
        Task<DataResult> GetAllMeterMonthlyAsync(GetAllMeterMonthlyDto input);

        Task<DataResult> GetMeterMonthlyByIdAsync(long id);
        Task<DataResult> CreateMeterMonthly(CreateMeterMonthlyInput input);
        Task<DataResult> UpdateMeterMonthly(UpdateMeterMonthlyInput input);
        Task<DataResult> DeleteMeterMonthly(long id);
        Task<DataResult> DeleteManyMeterMonthly([FromBody] List<long> ids);
    }

    public class AdminMeterMonthlyAppService : YootekAppServiceBase, IAdminMeterMonthlyAppService
    {
        private readonly IRepository<MeterMonthly, long> _meterMonthlyRepository;
        private readonly IRepository<Meter, long> _meterRepository;
        private readonly IRepository<MeterType, long> _meterTypeRepository;
        // private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<BillConfig, long> _billConfigRepository;
        private readonly IRepository<UserBill, long> _userBillRepo;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<CitizenTemp, long> _citizenTempRepo;
        private readonly IRepository<Apartment, long> _apartmentRepository;
        // private readonly IMeterMonthlyExcelExport _meterMonthlyExcelExport;

        public AdminMeterMonthlyAppService(
            IRepository<MeterMonthly, long> meterMonthlyRepository,
            IRepository<Meter, long> meterRepository,
            IRepository<MeterType, long> meterTypeRepository,
            // IRepository<AppOrganizationUnit, long> organizationUnitRepository,
            IRepository<BillConfig, long> billConfigRepository,
            IRepository<User, long> userRepository,
            IRepository<UserBill, long> userBillRepo,
            IRepository<CitizenTemp, long> citizenTempRepo,
            IRepository<Apartment, long> apartmentRepository
            // IMeterMonthlyExcelExport meterMonthlyExcelExport
            )
        {
            _meterMonthlyRepository = meterMonthlyRepository;
            _meterRepository = meterRepository;
            // _organizationUnitRepository = organizationUnitRepository;
            _userRepository = userRepository;
            _userBillRepo = userBillRepo;
            _apartmentRepository = apartmentRepository;
            // _meterMonthlyExcelExport = meterMonthlyExcelExport;
            _meterTypeRepository = meterTypeRepository;
            _billConfigRepository = billConfigRepository;
            _citizenTempRepo = citizenTempRepo;
        }


        public async Task<DataResult> GetAllMeterMonthlyAsync(GetAllMeterMonthlyDto input)
        {
            try
            {
                //if (!input.FromMonth.HasValue)
                //{
                //    // Nếu không có giá trị FromMonth, gán giá trị mặc định là đầu tháng của tháng hiện tại
                //    input.FromMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                //}

                //if (!input.ToMonth.HasValue)
                //{
                //    // Nếu không có giá trị ToMonth, gán giá trị mặc định là cuối tháng của tháng hiện tại
                //    input.ToMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
                //}
                //DateTime newFromMonth = new DateTime(), newToMonth = new DateTime();
                //if (input.FromMonth.HasValue)
                //{
                //    newFromMonth = new DateTime(input.FromMonth.Value.Year, input.FromMonth.Value.Month, input.FromMonth.Value.Day, 0, 0, 0);
                //}

                //if (input.ToMonth.HasValue)
                //{
                //    newToMonth = new DateTime(input.ToMonth.Value.Year, input.ToMonth.Value.Month, input.ToMonth.Value.Day, 23, 59, 59);
                //}
                var month = input.Period.HasValue ? input.Period.Value.Month : 0;
                var year = input.Period.HasValue ? input.Period.Value.Year : 0;
                // List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();

                IQueryable<MeterMonthlyDto> query = (from sm in _meterMonthlyRepository.GetAll()
                                                     join meter in _meterRepository.GetAll() on sm.MeterId equals meter.Id into tb_mt
                                                     from meter in tb_mt.DefaultIfEmpty()
                                                     join apartment in _apartmentRepository.GetAll() on meter.ApartmentCode equals apartment.ApartmentCode into tb_apartment
                                                     from apartment in tb_apartment.DefaultIfEmpty()
                                                     select new MeterMonthlyDto
                                                     {
                                                         Id = sm.Id,
                                                         TenantId = sm.TenantId,
                                                         Period = sm.Period,
                                                         Value = sm.Value,
                                                         MeterId = sm.MeterId,
                                                         CreationTime = sm.CreationTime,
                                                         CreatorUserId = sm.CreatorUserId ?? 0,
                                                         MeterTypeId = meter.MeterTypeId,
                                                         Name = meter.Name,
                                                         ImageUrl = sm.ImageUrl,
                                                         ApartmentCode = meter.ApartmentCode,
                                                         BuildingId = meter.BuildingId,
                                                         UrbanId = meter.UrbanId,
                                                         // BuildingName = _organizationUnitRepository.GetAll().Where(o => o.Id == meter.BuildingId)
                                                         //     .Select(b => b.DisplayName).FirstOrDefault(),
                                                         // UrbanName = _organizationUnitRepository.GetAll().Where(o => o.Id == meter.UrbanId)
                                                         //     .Select(b => b.DisplayName).FirstOrDefault(),
                                                         FirstValue = sm.FirstValue,
                                                         IsClosed = sm.IsClosed,
                                                         BillConfig = apartment.BillConfig,
                                                         BillType = _meterTypeRepository.GetAll().Where(m => m.Id == meter.MeterTypeId).Select(m => m.BillType).FirstOrDefault(),
                                                     })
                    // .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                    .WhereIf(input.Period.HasValue, x => x.Period.Value.Month == month && x.Period.Value.Year == year)
                    .WhereIf(input.MeterTypeId != null, x => x.MeterTypeId == input.MeterTypeId)
                    .WhereIf(input.UrbanId != null, x => x.UrbanId == input.UrbanId)
                    .WhereIf(input.BuildingId != null, x => x.BuildingId == input.BuildingId)
                    .WhereIf(input.ApartmentCode != null, x => x.ApartmentCode == input.ApartmentCode)
                    .WhereIf(input.MeterId != null, x => x.MeterId == input.MeterId)
                    .WhereIf(input.MinValue.HasValue, x => x.Value >= input.MinValue)
                    .WhereIf(input.MaxValue.HasValue, x => x.Value <= input.MaxValue)
                    //.WhereIf(input.FromMonth.HasValue, x => x.Period >= newFromMonth)
                    //.WhereIf(input.ToMonth.HasValue, x => x.Period <= newToMonth)
                    .WhereIf(input.IsClosed.HasValue, x => x.IsClosed == input.IsClosed)
                    .OrderByDescending(x => x.CreationTime)
                    .ApplySearchFilter(input.Keyword, x => x.Name, x => x.ApartmentCode);


                List<MeterMonthlyDto> result = await query
                    .OrderBy(x => x.ApartmentCode)
                    .ThenByDescending(x => x.IsClosed)
                    .ThenByDescending(x => x.CreationTime)
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                foreach (var item in result)
                {
                    item.State = CalculateState(item);

                    if (item.BillConfig != null)
                    {
                        var billConfigList = JsonConvert.DeserializeObject<List<BillConfigProperties>>(item.BillConfig);


                        List<GetAllBillConfigDto> listBillConfig = billConfigList
                            .Select(billConfig =>
                                new GetAllBillConfigDto
                                {
                                    //Properties = JsonConvert.DeserializeObject<BillProperites>(billConfig.Properties),
                                    Properties = billConfig.Properties,
                                    BillType = billConfig.BillType
                                })
                            .ToList();
                        item.ListBillConfig = listBillConfig;
                    }
                    var contractorCitizen = _citizenTempRepo
                        .GetAll()
                        .Where(x => x.ApartmentCode == item.ApartmentCode && x.IsStayed == true && x.RelationShip == RELATIONSHIP.Contractor)
                        .OrderByDescending(x => x.OwnerGeneration)
                        .FirstOrDefault();

                    var normalCitizen = _citizenTempRepo
                        .GetAll()
                        .Where(x => x.ApartmentCode == item.ApartmentCode && x.IsStayed == true)
                        .FirstOrDefault();

                    item.CustomerName = contractorCitizen?.FullName ?? normalCitizen?.FullName ?? "";
                }
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> GetMeterMonthlyByIdAsync(long id)
        {
            try
            {
                IQueryable<MeterMonthlyDto> query = (from sm in _meterMonthlyRepository.GetAll()
                                                     where sm.Id == id
                                                     join meter in _meterRepository.GetAll() on sm.MeterId equals meter.Id into tb_mt
                                                     from meter in tb_mt.DefaultIfEmpty()
                                                     join user in _userRepository.GetAll() on sm.CreatorUserId equals user.Id into tb_u
                                                     from user in tb_u.DefaultIfEmpty()
                                                     join apartment in _apartmentRepository.GetAll() on meter.ApartmentCode equals apartment.ApartmentCode into tb_apartment
                                                     from apartment in tb_apartment.DefaultIfEmpty()
                                                     select new MeterMonthlyDto
                                                     {
                                                         Id = sm.Id,
                                                         TenantId = sm.TenantId,
                                                         Period = sm.Period,
                                                         Value = sm.Value,
                                                         MeterId = sm.MeterId,
                                                         CreationTime = sm.CreationTime,
                                                         CreatorUserId = sm.CreatorUserId ?? 0,
                                                         MeterTypeId = meter.MeterTypeId,
                                                         Name = meter.Name,
                                                         ImageUrl = sm.ImageUrl,
                                                         ApartmentCode = meter.ApartmentCode,
                                                         BuildingId = meter.BuildingId,
                                                         UrbanId = meter.UrbanId,
                                                         FirstValue = sm.FirstValue,
                                                         // BuildingName = _organizationUnitRepository.GetAll().Where(o => o.Id == meter.BuildingId)
                                                         //     .Select(b => b.DisplayName).FirstOrDefault(),
                                                         // UrbanName = _organizationUnitRepository.GetAll().Where(o => o.Id == meter.UrbanId)
                                                         //     .Select(b => b.DisplayName).FirstOrDefault(),
                                                         CreatorUserName = user.FullName,


                                                     }).AsQueryable();

                var result = query.FirstOrDefault();

                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> CreateMeterMonthly(CreateMeterMonthlyInput input)
        {
            try
            {
                MeterMonthly meterMonthly = ObjectMapper.Map<MeterMonthly>(input);
                meterMonthly.TenantId = AbpSession.TenantId;

                // meterMonthly.FirstValue = input.FirstValue ?? (int?)_userBillRepo.GetAll().Where(u => u.ApartmentCode == input.ApartmentCode && u.Period.Value.Month != input.Period.Value.Month && u.Period.Value.Year != input.Period.Value.Year).Select(u => u.IndexEndPeriod).FirstOrDefault() ?? 0;

                if (input.Period != null)
                {
                    var meter = _meterRepository.FirstOrDefault(x => x.Id == input.MeterId);
                    if (meter != null)
                    {
                        var typeBill = _meterTypeRepository.FirstOrDefault(x => x.Id == meter.MeterTypeId)?.BillType;
                        var apartmentCode = input.ApartmentCode ?? meter.ApartmentCode;
                        if (typeBill != null)
                        {

                            var billCurr = _userBillRepo.GetAll()
                                .Where(x => x.ApartmentCode == apartmentCode && x.BillType == typeBill && x.Period <= input.Period)
                                .OrderByDescending(x => x.Period)
                                .ThenByDescending(x => x.CreationTime)
                                .FirstOrDefault();

                            meterMonthly.FirstValue = (int?)(input.FirstValue ?? billCurr?.IndexEndPeriod);

                        }
                    }
                }


                //chi lay year va month
                //meterMonthly.Period = new DateTime(meterMonthly.Period.Value.Year, meterMonthly.Period.Value.Month, 1, 0, 0, 0);

                await _meterMonthlyRepository.InsertAsync(meterMonthly);
                return DataResult.ResultSuccess(true, "Insert success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<DataResult> UpdateMeterMonthly(UpdateMeterMonthlyInput input)
        {
            try
            {
                MeterMonthly? updateData = await _meterMonthlyRepository.FirstOrDefaultAsync(input.Id)
                                           ?? throw new Exception("MeterMonthly not found!");
                MeterMonthly meterMonthly = ObjectMapper.Map(input, updateData);
                //chi lay year va month
                meterMonthly.Period = new DateTime(meterMonthly.Period.Value.Year, meterMonthly.Period.Value.Month, 1, 0, 0, 0);
                if (input.IsClosed == true)
                {
                    var listMeterIds = _meterMonthlyRepository.GetAll()
                        .Where(x => x.MeterId == input.MeterId && x.Id != input.Id)
                        .Select(x => x.Id)
                        .ToList();

                    await DeleteManyMeterMonthly(listMeterIds);
                }
                await _meterMonthlyRepository.UpdateAsync(updateData);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteMeterMonthly(long id)
        {
            try
            {
                await _meterMonthlyRepository.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> IsClosedMetersMonthly([FromBody] List<long> ids)
        {
            try
            {
                var metersToClose = await _meterMonthlyRepository.GetAll()
                    .Where(x => ids.Contains(x.Id))
                    .ToListAsync();

                var metersToCloseDistinct = metersToClose
                    .GroupBy(x => x.MeterId)
                    .Select(group => group.OrderByDescending(x => x.CreationTime).First())
                    .ToList();

                foreach (var meter in metersToCloseDistinct)
                {
                    if ((bool)!meter.IsClosed)
                    {
                        meter.IsClosed = true;

                        await _meterMonthlyRepository.UpdateAsync(meter);

                        var listMeterIds = _meterMonthlyRepository.GetAll()
                            .Where(x => x.MeterId == meter.MeterId && x.Id != meter.Id)
                            .Select(x => x.Id)
                            .ToList();

                        await DeleteManyMeterMonthly(listMeterIds);
                    }
                }

                return DataResult.ResultSuccess("Update success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }



        public async Task<DataResult> DeleteManyMeterMonthly([FromBody] List<long> ids)
        {
            try
            {
                await _meterMonthlyRepository.DeleteAsync(x => ids.Contains(x.Id));
                return DataResult.ResultSuccess("Delete list success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        // public async Task<object> ExportMeterMonthlyExcel(ExportMeterMonthlyDto input)
        // {
        //     try
        //     {
        //         List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
        //         var query = (from sm in _meterMonthlyRepository.GetAll()
        //                      join meter in _meterRepository.GetAll() on sm.MeterId equals meter.Id into tb_mt
        //                      from meter in tb_mt.DefaultIfEmpty()
        //                      select new MeterMonthlyDto
        //                      {
        //                          Id = sm.Id,
        //                          TenantId = sm.TenantId,
        //                          Period = sm.Period,
        //                          FirstValue = sm.FirstValue ?? (int?)_userBillRepo.GetAll().Where(u => u.ApartmentCode == meter.ApartmentCode).Select(u => u.IndexEndPeriod).FirstOrDefault() ?? 0,
        //                          Value = sm.Value,
        //                          MeterId = sm.MeterId,
        //                          CreationTime = sm.CreationTime,
        //                          CreatorUserId = sm.CreatorUserId ?? 0,
        //                          MeterTypeId = meter.MeterTypeId,
        //                          Name = meter.Name,
        //                          ImageUrl = sm.ImageUrl,
        //                          ApartmentCode = meter.ApartmentCode,
        //                          BuildingId = meter.BuildingId,
        //                          UrbanId = meter.UrbanId,
        //                          BuildingName = _organizationUnitRepository.GetAll().Where(o => o.Id == meter.BuildingId)
        //                              .Select(b => b.DisplayName).FirstOrDefault(),
        //                          UrbanName = _organizationUnitRepository.GetAll().Where(o => o.Id == meter.UrbanId)
        //                              .Select(b => b.DisplayName).FirstOrDefault(),
        //                      })
        //             .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
        //             .WhereIf(input.Ids != null && input.Ids.Count > 0, x => input.Ids.Contains(x.Id))
        //             .WhereIf(input.MeterTypeId.HasValue, x => x.MeterTypeId == input.MeterTypeId)
        //             .AsQueryable();
        //
        //         var meterMonthly = await query.ToListAsync();
        //         var result = _meterMonthlyExcelExport.ExportMeterMonthlyToExcel(meterMonthly);
        //         return DataResult.ResultSuccess(result, "Export Success");
        //     }
        //     catch (Exception e)
        //     {
        //         var data = DataResult.ResultError(e.ToString(), "Exception !");
        //         Logger.Fatal(e.Message);
        //         throw;
        //     }
        // }


        // public async Task<object> ImportMeterMonthlyExcel([FromForm] ImportMeterMonthlyInput input)
        // {
        //     try
        //     {
        //         using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
        //         {
        //             IFormFile file = input.File;
        //             string fileName = file.FileName;
        //             string fileExt = Path.GetExtension(fileName);
        //             if (fileExt != ".xlsx" && fileExt != ".xls")
        //             {
        //                 return DataResult.ResultError("File not supported", "Error");
        //             }
        //
        //             string filePath = Path.GetRandomFileName() + fileExt;
        //
        //             using (FileStream stream = File.Create(filePath))
        //             {
        //                 await file.CopyToAsync(stream);
        //                 stream.Close();
        //             }
        //
        //             var package = new ExcelPackage(new FileInfo(filePath));
        //             var worksheet = package.Workbook.Worksheets.First();
        //             int rowCount = worksheet.Dimension.End.Row;
        //
        //             const int APARTMENT_CODE_INDEX = 1;
        //             const int METER_CODE_INDEX = 2;
        //             const int FIRST_VALUE_INDEX = 3;
        //             const int VALUE_INDEX = 4;
        //             const int PERIOD_INDEX = 5;
        //             const int URBAN_CODE_INDEX = 6;
        //             const int BUILDING_CODE_INDEX = 7;
        //             // const int TYPE_CODE_INDEX = 7;
        //
        //             var listNew = new List<CreateMeterMonthlyInput>();
        //
        //             for (var row = 2; row <= rowCount; row++)
        //             {
        //                 var meterMonthly = new CreateMeterMonthlyInput();
        //                 string apartmentCode = worksheet.Cells[row, APARTMENT_CODE_INDEX].Text.Trim();
        //                 string meterCode = worksheet.Cells[row, METER_CODE_INDEX].Text?.Trim();
        //
        //                 var valueCell = worksheet.Cells[row, VALUE_INDEX];
        //                 var firstValueCell = worksheet.Cells[row, FIRST_VALUE_INDEX];
        //                 var value = !string.IsNullOrWhiteSpace(valueCell.Text) ? (decimal?)decimal.Parse(valueCell.Text) : null;
        //                 var firstValue = !string.IsNullOrWhiteSpace(firstValueCell.Text) ? (decimal?)decimal.Parse(firstValueCell.Text) : null;
        //                 var periodString = worksheet.Cells[row, PERIOD_INDEX].Text?.Trim();
        //                 var period = DateTime.ParseExact(periodString, "dd/MM/yyyy",
        //                           CultureInfo.InvariantCulture);
        //
        //                 string buildingCode = worksheet.Cells[row, BUILDING_CODE_INDEX].Text?.Trim();
        //                 string urbanCode = worksheet.Cells[row, URBAN_CODE_INDEX].Text.Trim();
        //
        //                 // meterMonthly.TenantId = AbpSession.TenantId;
        //                 meterMonthly.Period = period;
        //                 meterMonthly.IsClosed = false;
        //                 meterMonthly.Value = (int?)value;
        //                 meterMonthly.FirstValue = (int?)firstValue;
        //                 meterMonthly.ApartmentCode = apartmentCode;
        //
        //
        //                 var listBuilding = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.BUILDING);
        //                 var listUrban = _organizationUnitRepository.GetAllList(x => x.Type == APP_ORGANIZATION_TYPE.URBAN);
        //                 if (!string.IsNullOrEmpty(apartmentCode))
        //                 {
        //                     var building = listBuilding.FirstOrDefault(x => x.ProjectCode == buildingCode);
        //                     var urban = listUrban.FirstOrDefault(x => x.ProjectCode == urbanCode);
        //                     if (urban != null)
        //                     {
        //                         var meter = (from m in _meterRepository.GetAll()
        //                                      join mt in _meterTypeRepository.GetAll() on m.MeterTypeId equals mt.Id into tb_mt
        //                                      from mt in tb_mt.DefaultIfEmpty()
        //                                      where m.ApartmentCode == apartmentCode
        //                                            && m.UrbanId == urban.ParentId
        //                                            && m.BuildingId == building.ParentId
        //                                            && m.Code == meterCode
        //                                            && mt != null
        //                                      select m).FirstOrDefault();
        //
        //
        //
        //                         if (meter != null)
        //                         {
        //                             meterMonthly.MeterId = meter.Id;
        //
        //                         }
        //                     }
        //
        //
        //                 }
        //                 // if (firstValue == 0 && meterMonthly.Period != null)
        //                 // {
        //                 //     var pre_period = meterMonthly.Period.AddMonths(-1);
        //                 //     var pre_bill = _userBillRepo.FirstOrDefault(x =>
        //                 //            x.ApartmentCode == apartmentCode && x.Period.Value.Year == pre_period.Year &&
        //                 //            x.Period.Value.Month == pre_period.Month);
        //
        //                 //     meterMonthly.FirstValue = meterMonthly.FirstValue > 0 ? meterMonthly.FirstValue : (int)(pre_bill?.IndexEndPeriod ?? 0);
        //                 // }
        //
        //
        //                 listNew.Add(meterMonthly);
        //             }
        //
        //             await CreateListMeterMonthlyAsync(listNew);
        //
        //             // Xóa tệp đã tạo
        //             File.Delete(filePath);
        //
        //             return DataResult.ResultSuccess(listNew, "Upload success");
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         var data = DataResult.ResultError(ex.Message, "Error");
        //         Logger.Fatal(ex.Message, ex);
        //         throw;
        //     }
        // }

        private async Task CreateListMeterMonthlyAsync(List<CreateMeterMonthlyInput> input)
        {
            try
            {
                if (input == null || !input.Any())
                {
                    return;
                }
                foreach (var m in input)
                {
                    await CreateMeterMonthly(m);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                var data = DataResult.ResultError(ex.ToString(), "Exception !");
                Logger.Fatal(ex.Message);
                throw;
            }
        }

        #region Cảnh báo
        private int CalculateState(MeterMonthlyDto sm)
        {
            var bills = _userBillRepo.GetAll()
                .Where(u => u.ApartmentCode == sm.ApartmentCode
                    && u.Period > sm.Period.Value.AddMonths(-3)
                    && u.Period < sm.Period)
                .ToList();

            double? averageTotalIndex = bills.Any() ? bills.Average(u => (double?)u.TotalIndex) : 0;

            if (averageTotalIndex == null)
            {
                return 4; // State 4
            }
            else if (averageTotalIndex < (sm.Value - sm.FirstValue) * (1 / 1.1))
            {
                return 1; // State 1
            }
            else if (averageTotalIndex > (sm.Value - sm.FirstValue) / 10)
            {
                return 2; // State 2
            }
            else
            {
                return 3; // State 3
            }
        }

        #endregion
    }
}