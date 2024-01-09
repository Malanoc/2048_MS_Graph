namespace _2048_MS_Graph
{
    partial class Form1
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panelJeu = new System.Windows.Forms.Panel();
            this.lblScore = new System.Windows.Forms.Label();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnRestart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // panelJeu
            // 
            this.panelJeu.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelJeu.Location = new System.Drawing.Point(122, 119);
            this.panelJeu.Name = "panelJeu";
            this.panelJeu.Size = new System.Drawing.Size(480, 429);
            this.panelJeu.TabIndex = 0;
            this.panelJeu.TabStop = true;
            this.panelJeu.Paint += new System.Windows.Forms.PaintEventHandler(this.panelJeu_Paint);
            // 
            // lblScore
            // 
            this.lblScore.AutoSize = true;
            this.lblScore.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblScore.Location = new System.Drawing.Point(283, 52);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(0, 25);
            this.lblScore.TabIndex = 1;
            // 
            // btnHelp
            // 
            this.btnHelp.BackColor = System.Drawing.Color.PeachPuff;
            this.btnHelp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHelp.Image = global::_2048_MS_Graph.Properties.Resources.btn_info5050;
            this.btnHelp.Location = new System.Drawing.Point(615, 42);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(55, 55);
            this.btnHelp.TabIndex = 10;
            this.btnHelp.TabStop = false;
            this.btnHelp.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this.btnHelp.UseVisualStyleBackColor = false;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btnRestart
            // 
            this.btnRestart.BackColor = System.Drawing.Color.MistyRose;
            this.btnRestart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRestart.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRestart.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnRestart.Location = new System.Drawing.Point(615, 139);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(84, 33);
            this.btnRestart.TabIndex = 3;
            this.btnRestart.TabStop = false;
            this.btnRestart.Text = "Recommencer";
            this.btnRestart.UseVisualStyleBackColor = false;
            this.btnRestart.Click += new System.EventHandler(this.btnRestart_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.PeachPuff;
            this.ClientSize = new System.Drawing.Size(720, 668);
            this.Controls.Add(this.btnRestart);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.lblScore);
            this.Controls.Add(this.panelJeu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "2048 - MS";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelJeu;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnRestart;
    }
}

