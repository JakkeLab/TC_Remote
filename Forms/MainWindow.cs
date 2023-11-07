using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCTableBuilder.TCcommands;
using Trimble.Connect.Desktop.API.Models;
using Trimble.Connect.Desktop.API.Projects;

namespace TCTableBuilder.Forms
{
    public partial class MainWindow : Form/*, INotifyPropertyChanged*/
    {
        //Constructor
        public MainWindow()
        {
            InitializeComponent();
            command = new TCcommand();
            this.ConnectionChangeHandler += new EventHandler(ConnectionStatusChange);
            this.ProjectNameLoadHandler += new EventHandler(ProjectNameLoad);

            AllowXMove = false;
            AllowYMove = false;
            AllowZMove = false;
            AllowRotateZ = false;
        }

        //Form Control EventHandlers
        private void cbX_CheckedChanged(object sender, EventArgs e)
        {
            if (cbX.Checked)
            {
                tbX.Enabled = true;
                btXInvert.Enabled = true;
                AllowXMove = true;
            }
            else
            {
                tbX.Enabled = false;
                btXInvert.Enabled = false;
                AllowXMove = false;
            }
        }

        private void cbY_CheckedChanged(object sender, EventArgs e)
        {
            if (cbY.Checked)
            {
                tbY.Enabled = true;
                btYInvert.Enabled = true;
                AllowYMove = true;
            }
            else
            {
                tbY.Enabled = false;
                btYInvert.Enabled = false;
                AllowYMove = false;
            }
        }

        private void cbZ_CheckedChanged(object sender, EventArgs e)
        {
            if (cbZ.Checked)
            {
                tbZ.Enabled = true;
                btZInvert.Enabled = true;
                AllowZMove = true;
            }
            else
            {
                tbZ.Enabled = false;
                btZInvert.Enabled = false;
                AllowZMove = false;
            }
        }

        private void cbR_CheckedChanged(object sender, EventArgs e)
        {
            if (cbR.Checked)
            {
                tbR.Enabled = true;
                btRInvert.Enabled = true;
                AllowRotateZ = true;
            }
            else
            {
                tbR.Enabled = false;
                btRInvert.Enabled = false;
                AllowRotateZ = false;
            }
        }

        private void btXInvert_Click(object sender, EventArgs e)
        {
            double result;
            bool parseResult = double.TryParse(tbX.Text, out result);
            if(cbX.Enabled && parseResult)
            {
                tbX.Text = (-result).ToString();
            }
            else
            {
                MessageBox.Show("값은 실수로 입력해 주세요.", "Error");
            }
        }

        private void btYInvert_Click(object sender, EventArgs e)
        {
            double result;
            bool parseResult = double.TryParse(tbY.Text, out result);
            if (cbY.Enabled && parseResult)
            {
                tbY.Text = (-result).ToString();
            }
            else
            {
                MessageBox.Show("값은 실수로 입력해 주세요.", "Error");
            }
        }

        private void btZInvert_Click(object sender, EventArgs e)
        {
            double result;
            bool parseResult = double.TryParse(tbZ.Text, out result);
            if (cbZ.Enabled && parseResult)
            {
                tbZ.Text = (-result).ToString();
            }
            else
            {
                MessageBox.Show("값은 실수로 입력해 주세요.", "Error");
            }
        }

        private void btRInvert_Click(object sender, EventArgs e)
        {
            double result;
            bool parseResult = double.TryParse(tbR.Text, out result);
            if (cbR.Enabled && parseResult)
            {
                tbR.Text = (-result).ToString();
            }
            else
            {
                MessageBox.Show("값은 실수로 입력해 주세요.", "Error");
            }
        }

        private void btMove_Click(object sender, EventArgs e)
        {
            var models = command.GetSelectedModels(this.ActiveProject);
            var transformX = TransformArg.SetTransform(TransformArg.TransformType.X, cbX.Checked, tbX.Text);
            var transformY = TransformArg.SetTransform(TransformArg.TransformType.Y, cbY.Checked, tbY.Text);
            var transformZ = TransformArg.SetTransform(TransformArg.TransformType.Z, cbZ.Checked, tbZ.Text);
            var transformR = TransformArg.SetTransform(TransformArg.TransformType.R, cbR.Checked, tbR.Text);

            if(transformX == null || transformY == null || transformZ == null || transformR == null)
            {
                MessageBox.Show("변경할 값들의 양식을 맞춰주세요", "Error");
            }
            else
            {
                TransformSet transformSet = new TransformSet(transformX, transformY, transformZ, transformR);
                command.SetPosition(models, transformSet);
            }

            if(models.Count == 0)
            {
                MessageBox.Show("선택된 모델이 없습니다.", "Error");
            }
        }

        private bool CheckValue(string inputValue)
        {
            double result;
            bool test = double.TryParse(inputValue, out result);
            if (test)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void btCloseRemote_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            int connectTryCount = 0;

            //연결 최대 5번 시도
            while(connectTryCount < 5)
            {
                bool connectResult = command.Connect();
                if(connectResult)
                {
                    this.IsConnected = connectResult;
                    break;
                }
                else
                {
                    this.IsConnected = connectResult;
                    connectTryCount++;
                    Thread.Sleep(1000);
                    continue;
                }
            }
        }

        private void btRefresh_Click(object sender, EventArgs e)
        {
            int loadTryCount = 0;
            //로드 최대 5번 시도
            while (loadTryCount < 5)
            {
                var activeProject = command.getActiveProject();
                if(activeProject != null)
                {
                    this.ActiveProject = activeProject;
                    break;
                }
                else
                {
                    this.ActiveProject = null;
                    Thread.Sleep(1000);
                    loadTryCount++;
                    continue;
                }
            }
        }

        //Command Member
        public TCcommand command;

        //Props
        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                ConnectionChangeHandler.Invoke(this, new EventArgs());
            }
        }

        private Project _activeProject;
        public Project ActiveProject
        {
            get { return _activeProject; }
            set
            {
                _activeProject = value;
                ProjectNameLoadHandler.Invoke(this, new EventArgs());
                if(_activeProject != null)
                {
                    this.ActiveProject.ModelObjectManager.ModelObjectSelectionChanged += this.SelectedModelsChanged;
                }
                else
                {
                    this.ActiveProject.ModelObjectManager.ModelObjectSelectionChanged -= this.SelectedModelsChanged;
                }
            }
        }


        private List<Model> _selectedModels = new List<Model>();
        public List<Model> SelectedModels
        {
            get { return _selectedModels; }
            set { _selectedModels = value; }
        }

        public void SelectedModelsChanged(object sender, EventArgs eventArgs)
        {
            //모델 리스트에 추가
            SelectedModels.Clear();
            var models = command.GetSelectedModels(this.ActiveProject);
            SelectedModels.AddRange(models);

            //모델 이름들 새로고침
            if(lbObjects.InvokeRequired)
            {
                lbObjects.Invoke(new MethodInvoker(delegate()
                {
                    RefreshModelNameList();
                }));
            }

            //모델 개수 효시
            if(lbObjCount.InvokeRequired)
            {
                lbObjCount.Invoke(new MethodInvoker(delegate ()
                {
                    lbObjCount.Text = $"({models.Count})";
                }));
            }
        }

        public void RefreshModelNameList()
        {
            lbObjects.Items.Clear();
            var objNames = command.GetSelectedObjectNames(this.ActiveProject);
            foreach (string objName in objNames)
            {
                lbObjects.Items.Add(objName);
            }
        }

        public bool AllowXMove { get; set; }
        public bool AllowYMove { get; set; }
        public bool AllowZMove { get; set; }
        public bool AllowRotateZ { get; set; }


        ////OnPropertyChanged
        //public event PropertyChangedEventHandler PropertyChanged;
        //public void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        //EventHandler
        public event EventHandler ConnectionChangeHandler;
        public void ConnectionStatusChange(object sender, EventArgs e)
        {
            if(this.IsConnected)
            {
                this.tbClientConnect.Text = "Connected";
            }
            else
            {
                this.tbClientConnect.Text = "Disconnected";
            }
        }

        public event EventHandler ProjectNameLoadHandler;
        public void ProjectNameLoad(object sender, EventArgs e)
        {
            if(ActiveProject != null)
            {
                tbCurrentProject.Text = ActiveProject.Name;
                cbX.Enabled = true;
                cbY.Enabled = true;
                cbZ.Enabled = true;
                cbR.Enabled = true;
            }
            else
            {
                tbCurrentProject.Text = string.Empty;
                cbX.Enabled = false;
                cbY.Enabled = false;
                cbZ.Enabled = false;
                cbR.Enabled = false;
            }
        }
    }
}
