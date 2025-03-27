using SA2CutsceneTextTool.Common;

namespace SA2CutsceneTextTool.CSV
{
    public class CsvMessage
    {
        public string EventID { get; set; }
        public string Character { get; set; }
        public string Centered { get; set; }
        public string Text { get; set; }

        public CsvMessage() { }

        public void GetCsvMessage(Message message, Scene scene)
        {
            string centered = message.Centered.HasValue ? message.Centered.ToString() : "";
            EventID = scene.EventID.ToString();
            Character = message.Character.ToString();
            Centered = centered;
            Text = message.Text;
        }
    }
}
