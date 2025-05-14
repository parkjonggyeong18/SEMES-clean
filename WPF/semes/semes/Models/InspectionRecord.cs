public class InspectionRecord
{
    public int PcbId { get; set; }
    public DateTime InspectionDate { get; set; }
    public string SerialNumber { get; set; }
    public int DefectCount { get; set; }
    public string IsGood => DefectCount == 0 ? "양품" : "불량";
}
