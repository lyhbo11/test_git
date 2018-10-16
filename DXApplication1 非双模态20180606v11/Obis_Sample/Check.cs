using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Obis_Sample.Patients;
using DevExpress.XtraBars.Ribbon;
using System.IO;
using DevExpress.Utils.Drawing;

namespace Obis_New
{
    public partial class Check : DevExpress.XtraEditors.XtraForm
    {
        public List<Patient> DiagnosedPatients;
        public DataTable dt = new DataTable("患者列表");
        public string userid;
        public string password;
        public GalleryItemGroup group1 = new GalleryItemGroup();

        public Check(List<Patient>Diagnosed)
        {
            InitializeComponent();
            DiagnosedPatients = Diagnosed;
            dt.Columns.Add("患者姓名", typeof(string));
            dt.Columns.Add("患者年龄", typeof(string));
            dt.Columns.Add("患者性别", typeof(string));
            //dt.Columns.Add("上传状态", typeof(string));
            for (int i = 0; i < this.DiagnosedPatients.Count; i++)
            {
                //    string Attitute = null;
                //    if (CheckUp.UploadList.Contains(DiagnosedPatients[i]))
                //    {
                //        Attitute = "正在上传";
                //    }
                //    else if(CheckUp.UploadedList.Contains(DiagnosedPatients[i]))
                //    {
                //        Attitute = "上传完成";
                //    }
                //    else
                //    {
                //        Attitute = "删除用户";
                //    }
                dt.Rows.Add(this.DiagnosedPatients[i].Patient_name, this.DiagnosedPatients[i].Patient_age, this.DiagnosedPatients[i].Patient_sex);
            }
            gridControl1.DataSource = dt;
            galleryControl1.Gallery.ItemImageLayout = ImageLayoutMode.ZoomInside;
            galleryControl1.Gallery.ImageSize = new Size(120, 90);
            galleryControl1.Gallery.ShowItemText = true;
            group1.Caption = "眼底照片";
            galleryControl1.Gallery.Groups.Add(group1);
        }

        private void gridView1_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {

            int index = this.gridView1.GetFocusedDataSourceRowIndex();
            //MessageBox.Show(this.DiagnosedPatients[index].Patient_name);
            string result = Environment.GetEnvironmentVariable("USERPROFILE") + "\\Documents\\orbis\\";
            string path = result + "Data\\" + DiagnosedPatients[index].Patient_ID;
            if (Directory.Exists(path))
            {
                group1.Items.Clear();
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] fil = dir.GetFiles();
                DirectoryInfo[] dii = dir.GetDirectories();
                int i = 0;
                foreach (FileInfo f in fil)
                {
                    i++;
                    FileStream fileStream = new FileStream(f.FullName, FileMode.Open);
                    Image temp = Image.FromStream(fileStream);
                    fileStream.Close();
                    string[] name = f.FullName.Split('_');
                    GalleryItem item = new GalleryItem(temp, i.ToString(), "");
                    item.Tag = f.FullName;
                    group1.Items.Add(item);
                }
            }
        }

        private void galleryControl1_Gallery_ItemDoubleClick(object sender, GalleryItemClickEventArgs e)
        {
            List<GalleryItem> LstArray = galleryControl1.Gallery.GetCheckedItems();
            if (LstArray.Count == 0)
            {
                MessageBox.Show("请先点击图片再进行操作");
            }
            else
            {
                Picture_Show show = new Picture_Show();
                show.Image_Path = LstArray[0].Tag.ToString();
                show.Picture_ShowInit();
                show.Visible = true;
            }
        }

        private void galleryControl1_Click(object sender, EventArgs e)
        {

        }
    }
}