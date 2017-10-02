namespace Calendar.NET
{
    internal class TodayButton : CoolButton
    {
        public TodayButton()
        {
            Size = new System.Drawing.Size(72, 29);
            ButtonText = "Today";
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // TodayButton
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ButtonFont = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "TodayButton";
            this.ResumeLayout(false);

        }
    }
}
