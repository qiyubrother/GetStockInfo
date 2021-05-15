
namespace StockKit
{
    partial class FrmMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnUpdateStockCodePool = new System.Windows.Forms.Button();
            this.btnGetStockIndustry = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnHistoryData = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnEditConnectionString = new System.Windows.Forms.Button();
            this.btnStockPool = new System.Windows.Forms.Button();
            this.btnRiskNDay = new System.Windows.Forms.Button();
            this.btnFailNDay = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnUpdateStockCodePool
            // 
            this.btnUpdateStockCodePool.Location = new System.Drawing.Point(6, 20);
            this.btnUpdateStockCodePool.Name = "btnUpdateStockCodePool";
            this.btnUpdateStockCodePool.Size = new System.Drawing.Size(169, 23);
            this.btnUpdateStockCodePool.TabIndex = 0;
            this.btnUpdateStockCodePool.Text = "更新股票代码池";
            this.btnUpdateStockCodePool.UseVisualStyleBackColor = true;
            this.btnUpdateStockCodePool.Click += new System.EventHandler(this.btnUpdateStockCodePool_Click);
            // 
            // btnGetStockIndustry
            // 
            this.btnGetStockIndustry.Location = new System.Drawing.Point(6, 49);
            this.btnGetStockIndustry.Name = "btnGetStockIndustry";
            this.btnGetStockIndustry.Size = new System.Drawing.Size(169, 23);
            this.btnGetStockIndustry.TabIndex = 2;
            this.btnGetStockIndustry.Text = "更新股票所属行业";
            this.btnGetStockIndustry.UseVisualStyleBackColor = true;
            this.btnGetStockIndustry.Click += new System.EventHandler(this.btnGetStockIndustry_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnHistoryData);
            this.groupBox1.Controls.Add(this.btnUpdateStockCodePool);
            this.groupBox1.Controls.Add(this.btnGetStockIndustry);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(182, 110);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "数据库维护";
            // 
            // btnHistoryData
            // 
            this.btnHistoryData.Location = new System.Drawing.Point(7, 78);
            this.btnHistoryData.Name = "btnHistoryData";
            this.btnHistoryData.Size = new System.Drawing.Size(169, 23);
            this.btnHistoryData.TabIndex = 5;
            this.btnHistoryData.Text = "更新股票历史数据";
            this.btnHistoryData.UseVisualStyleBackColor = true;
            this.btnHistoryData.Click += new System.EventHandler(this.btnHistoryData_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnEditConnectionString);
            this.groupBox2.Controls.Add(this.btnStockPool);
            this.groupBox2.Location = new System.Drawing.Point(217, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 109);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "系统维护";
            // 
            // btnEditConnectionString
            // 
            this.btnEditConnectionString.Location = new System.Drawing.Point(7, 19);
            this.btnEditConnectionString.Name = "btnEditConnectionString";
            this.btnEditConnectionString.Size = new System.Drawing.Size(187, 23);
            this.btnEditConnectionString.TabIndex = 0;
            this.btnEditConnectionString.Text = "编辑数据库连接";
            this.btnEditConnectionString.UseVisualStyleBackColor = true;
            this.btnEditConnectionString.Click += new System.EventHandler(this.btnEditConnectionString_Click);
            // 
            // btnStockPool
            // 
            this.btnStockPool.Location = new System.Drawing.Point(7, 48);
            this.btnStockPool.Name = "btnStockPool";
            this.btnStockPool.Size = new System.Drawing.Size(187, 23);
            this.btnStockPool.TabIndex = 0;
            this.btnStockPool.Text = "自定义股票池";
            this.btnStockPool.UseVisualStyleBackColor = true;
            this.btnStockPool.Click += new System.EventHandler(this.btnStockPool_Click);
            // 
            // btnRiskNDay
            // 
            this.btnRiskNDay.Location = new System.Drawing.Point(6, 20);
            this.btnRiskNDay.Name = "btnRiskNDay";
            this.btnRiskNDay.Size = new System.Drawing.Size(187, 23);
            this.btnRiskNDay.TabIndex = 5;
            this.btnRiskNDay.Text = "连续上涨";
            this.btnRiskNDay.UseVisualStyleBackColor = true;
            this.btnRiskNDay.Click += new System.EventHandler(this.btnRiskNDay_Click);
            // 
            // btnFailNDay
            // 
            this.btnFailNDay.Location = new System.Drawing.Point(6, 49);
            this.btnFailNDay.Name = "btnFailNDay";
            this.btnFailNDay.Size = new System.Drawing.Size(187, 23);
            this.btnFailNDay.TabIndex = 6;
            this.btnFailNDay.Text = "连续下跌";
            this.btnFailNDay.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnRiskNDay);
            this.groupBox3.Controls.Add(this.btnFailNDay);
            this.groupBox3.Location = new System.Drawing.Point(423, 13);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(200, 230);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "工具包";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "StockKit";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnUpdateStockCodePool;
        private System.Windows.Forms.Button btnGetStockIndustry;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnEditConnectionString;
        private System.Windows.Forms.Button btnStockPool;
        private System.Windows.Forms.Button btnHistoryData;
        private System.Windows.Forms.Button btnRiskNDay;
        private System.Windows.Forms.Button btnFailNDay;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}

