﻿using EDM_DB;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UserAccount.Models;
using Public.Controllers;

namespace UserAccount.Controllers
{
    public class ThongTinCaNhanController : RouteConfigController
    {
        // GET: TaiKhoanCaNhan
        private readonly string VIEW_PATH = "~/Views/Admin/_SystemSetting/UserAccount";

        public ActionResult Index()
        {
            return View($"{VIEW_PATH}/thongtincanhan/thongtincanhan.cshtml");
        }

        #region Lấy các danh sách dữ liệu
        [HttpPost]
        public ActionResult getThongTinCaNhan(Guid idNguoiDung)
        {
            tbNguoiDungExtend nguoiDung = get_NguoiDungs(loai: "single", str_idNguoiDungs: string.Format("'{0}'", idNguoiDung)).FirstOrDefault();
            ViewBag.nguoiDung = nguoiDung;
            return PartialView($"{VIEW_PATH}/thongtincanhan/thongtincanhan-crud.cshtml");
        }
        public List<tbNguoiDungExtend> get_NguoiDungs(string loai, string str_idNguoiDungs = "", bool layThongTinPhu = true)
        {
            List<tbNguoiDungExtend> nguoiDungs = new List<tbNguoiDungExtend>();
            if (loai == "all")
            {
                string getSql = $@"select * from tbNguoiDung where MaDonViSuDung = '{per.DonViSuDung.MaDonViSuDung}' and TrangThai != 0 order by NgayTao desc";
                nguoiDungs = db.Database.SqlQuery<tbNguoiDungExtend>(getSql).ToList();
            }
            else
            {
                if (str_idNguoiDungs != "")
                {
                    string getSql = $@"select * from tbNguoiDung where MaDonViSuDung = '{per.DonViSuDung.MaDonViSuDung}' and TrangThai != 0 and IdNguoiDung in ({str_idNguoiDungs}) order by NgayTao desc";
                    nguoiDungs = db.Database.SqlQuery<tbNguoiDungExtend>(getSql).ToList();
                };
            };
            if (layThongTinPhu)
            {
                foreach (tbNguoiDungExtend nguoiDung in nguoiDungs)
                {
                    nguoiDung.KieuNguoiDung = db.tbKieuNguoiDungs.FirstOrDefault(x => x.IdKieuNguoiDung == nguoiDung.IdKieuNguoiDung) ?? new tbKieuNguoiDung();
                    nguoiDung.CoCauToChuc = db.tbCoCauToChucs.FirstOrDefault(x => x.IdCoCauToChuc == nguoiDung.IdCoCauToChuc) ?? new tbCoCauToChuc();
                    nguoiDung.ChucVu = db.default_tbChucVu.FirstOrDefault(x => x.IdChucVu == nguoiDung.IdChucVu) ?? new default_tbChucVu();
                };
            }
            return nguoiDungs;
        }
        #endregion

        #region Cập nhật người dùng
        public bool kiemTra_NguoiDung(tbNguoiDung nguoiDung)
        {
            // Kiểm tra còn hồ sơ khác có trùng mã không
            tbNguoiDung nguoiDung_OLD = db.tbNguoiDungs.FirstOrDefault(x => x.TenDangNhap == nguoiDung.TenDangNhap && x.IdNguoiDung != nguoiDung.IdNguoiDung && x.TrangThai != 0 && x.MaDonViSuDung == per.DonViSuDung.MaDonViSuDung);
            if (nguoiDung_OLD == null) return false;
            return true;
        }
        [HttpPost]
        public ActionResult update_NguoiDung(string str_nguoiDung)
        {
            string status = "success";
            string mess = "Cập nhật bản ghi thành công";

            using (var scope = db.Database.BeginTransaction())
            {
                try
                {
                    string format = "dd/MM/yyyy";
                    IsoDateTimeConverter dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };
                    tbNguoiDungExtend nguoiDung_NEW = JsonConvert.DeserializeObject<tbNguoiDungExtend>(str_nguoiDung ?? "", dateTimeConverter);
                    if (nguoiDung_NEW == null)
                    {
                        status = "error";
                        mess = "Chưa có bản ghi nào";
                    }
                    else
                    {
                        if (kiemTra_NguoiDung(nguoiDung: nguoiDung_NEW))
                        {
                            status = "datontai";
                            mess = "Tên đăng nhập đã tồn tại";
                        }
                        else
                        {
                            tbNguoiDungExtend nguoiDung_OLD = db.Database.SqlQuery<tbNguoiDungExtend>($@"select * from tbNguoiDung where MaDonViSuDung = '{per.DonViSuDung.MaDonViSuDung}' and IdNguoiDung = '{nguoiDung_NEW.IdNguoiDung}'").FirstOrDefault();
                            if (nguoiDung_NEW.IdKieuNguoiDung != null || nguoiDung_NEW.IdKieuNguoiDung != Guid.Empty)
                            {
                                nguoiDung_NEW.KieuNguoiDung = db.tbKieuNguoiDungs.FirstOrDefault(x => x.IdKieuNguoiDung == nguoiDung_NEW.IdKieuNguoiDung) ?? new tbKieuNguoiDung();
                                nguoiDung_OLD.KieuNguoiDung = db.tbKieuNguoiDungs.FirstOrDefault(x => x.IdKieuNguoiDung == nguoiDung_OLD.IdKieuNguoiDung) ?? new tbKieuNguoiDung();

                            };
                            if (nguoiDung_NEW.IdCoCauToChuc != null || nguoiDung_NEW.IdCoCauToChuc != Guid.Empty)
                            {
                                nguoiDung_NEW.CoCauToChuc = db.tbCoCauToChucs.FirstOrDefault(x => x.IdCoCauToChuc == nguoiDung_NEW.IdCoCauToChuc) ?? new tbCoCauToChuc();
                                nguoiDung_OLD.CoCauToChuc = db.tbCoCauToChucs.FirstOrDefault(x => x.IdCoCauToChuc == nguoiDung_OLD.IdCoCauToChuc) ?? new tbCoCauToChuc();
                            };
                            if (nguoiDung_NEW.IdChucVu != null || nguoiDung_NEW.IdChucVu != Guid.Empty)
                            {
                                nguoiDung_NEW.ChucVu = db.default_tbChucVu.FirstOrDefault(x => x.IdChucVu == nguoiDung_NEW.IdChucVu) ?? new default_tbChucVu();
                                nguoiDung_OLD.ChucVu = db.default_tbChucVu.FirstOrDefault(x => x.IdChucVu == nguoiDung_OLD.IdChucVu) ?? new default_tbChucVu();
                            };
                        
                            string sql_capNhatNguoiDung = $@"
                            update tbNguoiDung set
                                TenNguoiDung = N'{nguoiDung_NEW.TenNguoiDung}',
                                GioiTinh = {(nguoiDung_NEW.GioiTinh.Value ? 1 : 0)},
                                Email = '{nguoiDung_NEW.Email}',
                                SoDienThoai = '{nguoiDung_NEW.SoDienThoai}',
                                SoTaiKhoanNganHang = '{nguoiDung_NEW.SoTaiKhoanNganHang}',
                                NgaySinh = '{nguoiDung_NEW.NgaySinh}',
                                GhiChu = N'{nguoiDung_NEW.GhiChu}',
                                LinkLienHe = '{nguoiDung_NEW.LinkLienHe}',

                                IdNguoiSua = '{per.NguoiDung.IdNguoiDung}',
                                NgaySua = '{DateTime.Now}'
                            where MaDonViSuDung = '{per.DonViSuDung.MaDonViSuDung}' and IdNguoiDung = '{nguoiDung_NEW.IdNguoiDung}'
                            ";
                            db.Database.ExecuteSqlCommand(sql_capNhatNguoiDung);
                            // Cập nhật lại session
                            //status = "logout";
                            //mess = "[Tài khoản đang sử dụng]";
                            per.NguoiDung = nguoiDung_OLD;

                            db.SaveChanges();
                            #region Gửi mail
                            string mail()
                            {
                                var model = new CapNhatTaiKhoanMailM<tbNguoiDungExtend>
                                {
                                    NguoiDung_OLD = nguoiDung_OLD,
                                    NguoiDung_NEW = nguoiDung_NEW,
                                    DonViSuDung = per.DonViSuDung
                                };
                                // Gọi phương thức RenderViewToString() để chuyển đổi view thành chuỗi
                                string viewAsString = Public.Handle.RenderViewToString(this, $"{VIEW_PATH}/useraccount-mail.capnhattaikhoan.cshtml", model);
                                // Trả về chuỗi đã được tạo ra từ view
                                return viewAsString;
                            }
                            string tieuDeMail = "[📣 VIETGEN] - CẬP NHẬT THÔNG TIN TÀI KHOẢN❗";
                            string mailBody = mail();
                            // Gửi mail
                            Public.Handle.SendEmail(sendTo: nguoiDung_OLD.Email, subject: tieuDeMail, body: mailBody, isHTML: true, donViSuDung: per.DonViSuDung);
                            if (nguoiDung_NEW.Email != nguoiDung_OLD.Email)
                            {
                                Public.Handle.SendEmail(sendTo: nguoiDung_NEW.Email, subject: tieuDeMail, body: mailBody, isHTML: true, donViSuDung: per.DonViSuDung);
                            };
                            #endregion
                            scope.Commit();
                        };
                    }
                }
                catch (Exception ex)
                {
                    status = "error";
                    mess = ex.Message;
                    scope.Rollback();
                }
            }
            return Json(new
            {
                status,
                mess
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult capNhat_MatKhau(string str_nguoiDung)
        {
            string status = "success";
            string mess = "Cập nhật bản ghi thành công";

            using (var scope = db.Database.BeginTransaction())
            {
                try
                {
                    string format = "dd/MM/yyyy";
                    IsoDateTimeConverter dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };
                    tbNguoiDungExtend nguoiDung_NEW = JsonConvert.DeserializeObject<tbNguoiDungExtend>(str_nguoiDung ?? "", dateTimeConverter);
                    if (nguoiDung_NEW == null)
                    {
                        status = "error";
                        mess = "Chưa có bản ghi nào";
                    }
                    else
                    {
                        tbNguoiDung nguoiDung_OLD = db.tbNguoiDungs.FirstOrDefault(x => x.MaDonViSuDung == per.DonViSuDung.MaDonViSuDung && x.IdNguoiDung == nguoiDung_NEW.IdNguoiDung);
                        string matKhau_MD5 = Public.Handle.HashToMD5(nguoiDung_NEW.MatKhauMoi);
                        if (nguoiDung_OLD.MatKhau != matKhau_MD5)
                        {
                            status = "matkhaucuchuachinhxac";
                            mess = "Mật khẩu cũ chưa chính xác";
                        }
                        else
                        {
                            // Kiểm tra độ bảo mật
                            var conditions = Public.Handle.CheckPassPattern(nguoiDung_NEW.MatKhauMoi);
                            // Kiểm tra từng điều kiện
                            foreach (var condition in conditions)
                            {
                                if (!condition.Value.status) return Json(new { status = "warning", mess = condition.Value.error });
                            };

                            //if (nguoiDung_NEW.MatKhauMoi != nguoiDung_NEW.MatKhauMoi) return Json(new { mess = "Mật khẩu xác nhận chưa trùng khớp" });

                            nguoiDung_OLD.MatKhau = matKhau_MD5;
                            // Cập nhật lại session
                            status = "logout";
                            mess = "[Tài khoản đang sử dụng]";
                            per.NguoiDung = nguoiDung_OLD;

                            db.SaveChanges();
                            #region Gửi mail
                            string mail()
                            {
                                var model = new CapNhatTaiKhoanMailM<tbNguoiDungExtend>
                                {
                                    NguoiDung_NEW = nguoiDung_NEW,
                                    DonViSuDung = per.DonViSuDung,
                                    HinhThucCapNhat = "doimatkhau"
                                };
                                // Gọi phương thức RenderViewToString() để chuyển đổi view thành chuỗi
                                string viewAsString = Public.Handle.RenderViewToString(this, $"{VIEW_PATH}/useraccount-mail.capnhattaikhoan.cshtml", model);
                                // Trả về chuỗi đã được tạo ra từ view
                                return viewAsString;
                            }
                            string tieuDeMail = "[📣 VIETGEN] - CẬP NHẬT THÔNG TIN TÀI KHOẢN❗";
                            string mailBody = mail();
                            // Gửi mail
                            Public.Handle.SendEmail(sendTo: nguoiDung_OLD.Email, subject: tieuDeMail, body: mailBody, isHTML: true, donViSuDung: per.DonViSuDung);
                            if (nguoiDung_NEW.Email != nguoiDung_OLD.Email)
                            {
                                Public.Handle.SendEmail(sendTo: nguoiDung_NEW.Email, subject: tieuDeMail, body: mailBody, isHTML: true, donViSuDung: per.DonViSuDung);
                            };
                            #endregion
                            scope.Commit();
                        };
                    };
                }
                catch (Exception ex)
                {
                    status = "error";
                    mess = ex.Message;
                    scope.Rollback();
                }
            }
            return Json(new
            {
                status,
                mess
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}