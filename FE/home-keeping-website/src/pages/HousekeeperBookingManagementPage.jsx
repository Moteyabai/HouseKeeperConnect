import React, { useEffect, useState } from "react";
import BookingCard from "../components/BookingCard";
import { Modal, Button } from "react-bootstrap";
import {
  FaMoneyBillWave,
  FaMapMarkerAlt,
  FaBriefcase,
  FaUser,
  FaCalendarAlt,
  FaFileAlt,
  FaClock,
  FaCheckCircle,
} from "react-icons/fa";
import { toast, ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { serviceMap } from "../utils/serviceMap";
import { toast, ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import BookingDetailModal from "../components/BookingDetailModal";

const dayNames = ["Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy"];
const slotMap = {
  1: "8H - 9H",
  2: "10H - 11H",
  3: "12H - 13H",
  4: "14H - 15H",
  5: "16H - 17H",
  6: "18H - 19H",
  7: "20H - 21H"
};

const HousekeeperBookingManagementPage = () => {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedBooking, setSelectedBooking] = useState(null);
  const [selectedDay, setSelectedDay] = useState("");
  const [showModal, setShowModal] = useState(false);
  const [matchedDate, setMatchedDate] = useState(null);
  const [isToday, setIsToday] = useState(false);

  const housekeeperID = localStorage.getItem("housekeeperID");
  const accountID = localStorage.getItem("accountID");
  const authToken = localStorage.getItem("authToken");

  const handleMarkComplete = async (jobID) => {
    if (!authToken || !accountID) {
      toast.error("Vui lòng đăng nhập lại.");
      return;
    }

    try {
      const res = await fetch(`http://localhost:5280/api/Job/HousekeeperCompleteJob?jobId=${jobID}&accountID=${accountID}`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${authToken}`,
          "Content-Type": "application/json"
        }
      });

      const msg = await res.text();
      if (res.ok) {
        toast.success(msg || "✅ Đã báo hoàn thành công việc!");

        setRows(prev =>
          prev.map(row =>
            row.jobID === jobID
              ? { ...row, status: 6 }
              : row
          )
        );
      } else {
        toast.error(msg || "❌ Không thể hoàn thành công việc.");
      }
    } catch (err) {
      toast.error("Lỗi khi gọi API.");
      console.error(err);
    }
  };

  useEffect(() => {
    const fetchData = async () => {
      try {
        const res = await fetch(`http://localhost:5280/api/Booking/GetBookingByHousekeeperID?housekeeperId=${housekeeperID}`, {
          headers: { Authorization: `Bearer ${authToken}` }
        });
        const bookingData = await res.json();

        const fullRows = await Promise.all(
          bookingData.map(async (booking) => {
            let jobDetail = null;
            let familyName = "Đang cập nhật";

            console.log(booking);
            try {
              const jobRes = await fetch(`http://localhost:5280/api/Job/GetJobDetailByID?id=${booking.jobID}`, {
                headers: { Authorization: `Bearer ${authToken}` }
              });
              jobDetail = await jobRes.json();

              const familyRes = await fetch(`http://localhost:5280/api/Families/GetFamilyByID?id=${jobDetail.familyID}`, {
                headers: { Authorization: `Bearer ${authToken}` }
              });
              const familyData = await familyRes.json();

              const accountRes = await fetch(`http://localhost:5280/api/Account/GetAccount?id=${familyData.accountID}`, {
                headers: { Authorization: `Bearer ${authToken}` }
              });
              const accountData = await accountRes.json();

              familyName = accountData.name;
            } catch (error) {
              console.warn("Some data missing", error);
            }

            return {
              jobID: booking.jobID,
              bookingID: booking.bookingID,
              jobID: jobDetail?.jobID, // ✅ add this line
              jobName: jobDetail?.jobName || "Đang cập nhật",
              familyName,
              status: booking.status,
              location: jobDetail?.location || "Đang cập nhật",
              price: jobDetail?.price ? `${jobDetail.price.toLocaleString()} VND` : "Đang cập nhật",
              startDate: jobDetail?.startDate ? new Date(jobDetail.startDate).toLocaleDateString("vi-VN") : "Đang cập nhật",
              endDate: jobDetail?.endDate ? new Date(jobDetail.endDate).toLocaleDateString("vi-VN") : "Đang cập nhật",
              description: jobDetail?.description || "Đang cập nhật",
              slot: Array.isArray(jobDetail?.slotIDs) ? jobDetail.slotIDs.map(s => slotMap[s] || `Slot ${s}`) : [],
              days: Array.isArray(jobDetail?.dayofWeek) ? jobDetail.dayofWeek.map(d => dayNames[d]) : [],
              services: Array.isArray(jobDetail?.serviceIDs) ? jobDetail.serviceIDs.map(id => serviceMap[id]) : []
            };
          })
        );

        setRows(fullRows);
      } catch (err) {
        console.error("Error fetching booking data:", err);
      } finally {
        setLoading(false);
      }
    };

    if (housekeeperID && authToken) fetchData();
  }, [housekeeperID, authToken]);

  return (
    <div className="container py-4">
      <ToastContainer position="top-center" autoClose={3000} />
      <h4 className="fw-bold mb-4 text-primary">📋 Danh sách đặt công việc</h4>

      {loading ? (
        <p className="text-muted">⏳ Đang tải dữ liệu...</p>
      ) : rows.length === 0 ? (
        <p className="text-muted">Không có công việc nào được đặt.</p>
      ) : (
        <div className="row g-3">
          {rows.map((row, idx) => (
            <div className="col-12" key={idx}>
              <div className="card shadow-sm border-0 rounded-3 p-3 mb-4">
                <div className="d-flex justify-content-between align-items-center mb-2">
                  <h6 className="fw-bold mb-0">
                    <FaBriefcase className="me-2 text-warning" />
                    {row.jobName}
                  </h6>
                  <span className="text-muted small">#{row.bookingID}</span>
                </div>

                <div className="text-muted small mb-2"><FaUser className="me-1" /><strong>Gia đình:</strong> {row.familyName}</div>
                <div className="small d-flex flex-wrap mb-1">
                  <div className="me-3"><FaMapMarkerAlt className="me-1 text-danger" /><strong>Địa điểm:</strong> {row.location}</div>
                  <div><FaMoneyBillWave className="me-1 text-success" /><strong>Lương:</strong> {row.price}</div>
                </div>
                <div className="small d-flex flex-wrap mb-1">
                  <div className="me-3"><FaCalendarAlt className="me-1 text-primary" /><strong>Bắt đầu:</strong> {row.startDate}</div>
                  <div><FaCalendarAlt className="me-1 text-danger" /><strong>Kết thúc:</strong> {row.endDate}</div>
                </div>
                <div className="small mb-2"><FaFileAlt className="me-1 text-secondary" /><strong>Mô tả:</strong> {row.description}</div>

                <div className="row small mb-3">
                  <div className="col-md-4"><strong><FaClock className="me-1 text-info" />Ca làm việc:</strong><ul className="ps-3 mb-0">{row.slot.map((s, i) => <li key={i} className="text-info">{s}</li>)}</ul></div>
                  <div className="col-md-4"><strong>📅 Thứ:</strong><ul className="ps-3 mb-0">{row.days.map((d, i) => <li key={i} className="text-warning">{d}</li>)}</ul></div>
                  <div className="col-md-4"><strong>🛎️ Dịch vụ:</strong><ul className="ps-3 mb-0">{row.services.map((s, i) => <li key={i} className="text-success">{s}</li>)}</ul></div>
                </div>

                <div className="text-end">
                  {row.status === 3 ? (
                    <button
                      className="btn btn-sm btn-success rounded-pill fw-bold"
                      onClick={() => handleMarkComplete(row.jobID)}
                    >
                      <FaCheckCircle className="me-1" />
                      Báo hoàn thành
                    </button>
                  ) : row.status === 4 ? (
                    <span className="badge bg-success px-3 py-2 rounded-pill">
                      Đã hoàn thành ✅
                    </span>
                  ) : row.status === 2 ? (
                    <span className="badge bg-secondary px-3 py-2 rounded-pill">
                      Đang thực hiện
                    </span>
                  ) : row.status === 6 ? null : (
                    <span className="badge bg-light text-dark px-3 py-2 rounded-pill">
                      Trạng thái không xác định
                    </span>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <BookingDetailModal
        show={showModal}
        onClose={() => setShowModal(false)}
        booking={selectedBooking}
        selectedDay={selectedDay}
        matchedDate={matchedDate}
        isToday={isToday}
        onCheckIn={handleCheckIn}
      />

    </div>
  );
};

export default HousekeeperBookingManagementPage;
