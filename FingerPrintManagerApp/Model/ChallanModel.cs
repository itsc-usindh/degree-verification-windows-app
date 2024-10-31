using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ChallanRec
{
    [JsonProperty("CHALLAN_ID")]
    public string ChallanId { get; set; }

    [JsonProperty("EXAM_YEAR")]
    public string ExamYear { get; set; }

    [JsonProperty("SEAT_NO")]
    public string SeatNo { get; set; }

    [JsonProperty("DATE")]
    public DateTime Date { get; set; }

    [JsonProperty("AMOUNT")]
    public decimal Amount { get; set; }

    [JsonProperty("NAME")]
    public string Name { get; set; }

    [JsonProperty("FNAME")]
    public string Fname { get; set; }

    [JsonProperty("SURNAME")]
    public string Surname { get; set; }

    [JsonProperty("REMARKS")]
    public string Remarks { get; set; }

    [JsonProperty("PROG_LIST_ID")]
    public string ProgListId { get; set; }

    [JsonProperty("PART_FEE_ID")]
    public string PartFeeId { get; set; }

    [JsonProperty("CNIC_NO")]
    public string CnicNo { get; set; }

    [JsonProperty("MOBILE_NO")]
    public string MobileNo { get; set; }

    [JsonProperty("EMAIL")]
    public string Email { get; set; }

    [JsonProperty("PART")]
    public string Part { get; set; }

    [JsonProperty("FILE_NAME")]
    public string FileName { get; set; }

    [JsonProperty("IS_USED")]
    public string IsUsed { get; set; }

    [JsonProperty("USED_BY")]
    public string UsedBy { get; set; }

    [JsonProperty("USED_DATE")]
    public DateTime? UsedDate { get; set; }

    [JsonProperty("PROGRAM_TITLE")]
    public string ProgramTitle { get; set; }

    [JsonProperty("PROG_CODE")]
    public string ProgCode { get; set; }

    [JsonProperty("PRE_REQ_PER")]
    public string PreReqPer { get; set; }

    [JsonProperty("SYSTEM_ID")]
    public string SystemId { get; set; }

    [JsonProperty("FEES_TYPE_ID")]
    public string FeesTypeId { get; set; }

    [JsonProperty("TYPE_CODE")]
    public string TypeCode { get; set; }

    [JsonProperty("TITLE")]
    public string Title { get; set; }

    [JsonProperty("SEMESTER")]
    public string Semester { get; set; }

    [JsonProperty("PART_FEES")]
    public string PartFees { get; set; }

    [JsonProperty("EXAM_TYPE")]
    public string ExamType { get; set; }

    [JsonProperty("DEGREE_TYPE")]
    public string DegreeType { get; set; }

    [JsonProperty("BOARD_TYPE_ID")]
    public string BoardTypeId { get; set; }

    [JsonProperty("ACCOUNT_NO")]
    public string AccountNo { get; set; }

    [JsonProperty("DEPT_TYPE")]
    public string DeptType { get; set; }

    [JsonProperty("BATCH_YEAR")]
    public string BatchYear { get; set; }

    [JsonProperty("ORDER_BY_SNO")]
    public string OrderBySno { get; set; }

    [JsonProperty("ACTIVE")]
    public string Active { get; set; }
}

public class CertificateDeliveryStatus
{
    [JsonProperty("STATUS_ID")]
    public string StatusId { get; set; }

    [JsonProperty("STATUS_NAME")]
    public string StatusName { get; set; }

    [JsonProperty("REMARKS")]
    public string Remarks { get; set; }
}

public class OnlinePaidStatus
{
    [JsonProperty("ID")]
    public string Id { get; set; }

    [JsonProperty("DESCRIPTION")]
    public string Description { get; set; }

    [JsonProperty("LONG_DESCRIPTION")]
    public string LongDescription { get; set; }

    [JsonProperty("p_ConsumerNumber")]
    public string ConsumerNumber { get; set; }

    [JsonProperty("p_IsPaid")]
    public string IsPaid { get; set; }

    [JsonProperty("p_CustomerName")]
    public string CustomerName { get; set; }

    [JsonProperty("p_Amount")]
    public decimal Amount { get; set; }

    [JsonProperty("p_DueDate")]
    public DateTime DueDate { get; set; }

    [JsonProperty("p_AmountAfterDueDate")]
    public decimal AmountAfterDueDate { get; set; }

    [JsonProperty("p_BillingMonth")]
    public string BillingMonth { get; set; }

    [JsonProperty("p_CustomerFatherName")]
    public string CustomerFatherName { get; set; }

    [JsonProperty("p_CustomerRollNo")]
    public string CustomerRollNo { get; set; }

    [JsonProperty("p_Description")]
    public string P_Description { get; set; }

    [JsonProperty("p_Surname")]
    public string Surname { get; set; }

    [JsonProperty("p_Program")]
    public string Program { get; set; }

    [JsonProperty("p_TransactionId")]
    public string TransactionId { get; set; }

    [JsonProperty("p_PaidDate")]
    public DateTime PaidDate { get; set; }

    [JsonProperty("p_PaidAmount")]
    public decimal PaidAmount { get; set; }

    [JsonProperty("p_Channel")]
    public string Channel { get; set; }
}

public class BookingDetail
{
    [JsonProperty("STATUS_DATE")]
    public DateTime StatusDate { get; set; }

    [JsonProperty("BOOKING_ID")]
    public string BookingId { get; set; }

    [JsonProperty("STATUS_ID")]
    public string StatusId { get; set; }

    [JsonProperty("CHALLAN_NO")]
    public string ChallanNo { get; set; }

    [JsonProperty("SEAT_NO")]
    public string SeatNo { get; set; }

    [JsonProperty("EXAM_YEAR")]
    public string ExamYear { get; set; }

    [JsonProperty("CNIC_NO")]
    public string CnicNo { get; set; }

    [JsonProperty("NAME")]
    public string Name { get; set; }

    [JsonProperty("FNAME")]
    public string Fname { get; set; }

    [JsonProperty("SURNAME")]
    public string Surname { get; set; }

    [JsonProperty("MOBILE_NO")]
    public string MobileNo { get; set; }

    [JsonProperty("EMAIL_ADDRESS")]
    public string EmailAddress { get; set; }

    [JsonProperty("AMOUNT")]
    public decimal Amount { get; set; }

    [JsonProperty("PAID_DATE")]
    public DateTime PaidDate { get; set; }

    [JsonProperty("BOOKING_DATE")]
    public DateTime BookingDate { get; set; }

    [JsonProperty("DELIVERY_DATE")]
    public DateTime DeliveryDate { get; set; }

    [JsonProperty("PROG_LIST_ID")]
    public string ProgListId { get; set; }

    [JsonProperty("PART_FEE_ID")]
    public string PartFeeId { get; set; }

    [JsonProperty("PART")]
    public string Part { get; set; }

    [JsonProperty("FILE_NAME")]
    public string FileName { get; set; }

    [JsonProperty("REMARKS")]
    public string Remarks { get; set; }

    [JsonProperty("PAID_VIA")]
    public string PaidVia { get; set; }

    [JsonProperty("CERTIFICATE_DESC")]
    public string CertificateDesc { get; set; }

    [JsonProperty("IS_PAYMENT_VERIFIED")]
    public string IsPaymentVerified { get; set; }

    [JsonProperty("STATUS_NAME")]
    public string StatusName { get; set; }

    [JsonProperty("PROGRAM_TITLE")]
    public string ProgramTitle { get; set; }

    [JsonProperty("CERT_NO")]
    public string CertNo { get; set; }

    [JsonProperty("RECEIVER_NAME")]
    public string ReceiverName { get; set; }

    [JsonProperty("RECEIVER_CNIC_NO")]
    public string ReceiverCnicNo { get; set; }

    [JsonProperty("DELIVERED_BY")]
    public string DeliveredBy { get; set; }
}

public class ChallanModel
{
    [JsonProperty("CHALLAN_REC")]
    public ChallanRec ChallanRec { get; set; }

    [JsonProperty("BOOKING_DETAIL")]
    public BookingDetail BookingDetail { get; set; }

    [JsonProperty("CERTIFICATE_DELIVERY_STATUS")]
    public List<CertificateDeliveryStatus> CertificateDeliveryStatus { get; set; }

    [JsonProperty("ONLINE_PAID_STATUS")]
    public OnlinePaidStatus OnlinePaidStatus { get; set; }
}
