using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Trimble.Connect.Desktop.API;
using Trimble.Connect.Desktop.API.Attributes;
using Trimble.Connect.Desktop.API.Filters;
using Trimble.Connect.Desktop.API.ModelObjects;
using Trimble.Connect.Desktop.API.Models;
using Trimble.Connect.Desktop.API.Projects;
using static TCTableBuilder.TCcommands.TransformArg;

namespace TCTableBuilder.TCcommands
{
    public class TCcommand
    {
        private readonly TrimbleConnectDesktopClient client = new TrimbleConnectDesktopClient();
        private bool connected = false;

        public bool Connect()
        {
            connected = this.client.Connect();
            return this.client.Connect();
        }

        public void Disconnect()
        {
            this.client.Disconnect();
        }

        public Project getActiveProject()
        {
            if (this.connected)
            {
                var activeProject = this.client.ProjectManager.GetActiveProject();
                return activeProject;
            }
            else
            {
                return null;
            }
        }

        public void MoveObjects(Project project, double moveX, double moveY, double moveZ)
        {
            var models = project.ModelManager.GetLoadedModels().ToList();
            foreach (Model model in models)
            {
                model.SetPosition(moveX, moveY, moveZ);
            }
        }

        public List<string> GetSelectedObjectNames(Project project)
        {
            var result = new List<string>();
            SelectionFilter filter = new SelectionFilter(true);

            //Get All Models as Model Id and name
            List<Model> models = project.ModelManager.GetAllModels()
                .ToList();
            List<ModelIdentifier> modelInfos = models
                .Select(x => new ModelIdentifier(x.Identifier, x.Name))
                .ToList();
        
            //Get Unique model Ids
            List<ModelObject> modelObjects = project.ModelObjectManager
                .GetModelObjects(filter, Trimble.Connect.Desktop.API.Common.ObjectSelectionMode.HighestLevelAssembliesAndSystems)
                .ToList();
            List<string> uniqueIds = modelObjects
                .Select(x => x.ModelIdentifier)
                .Distinct()
                .ToList();
            
            foreach(string uniqueId in uniqueIds)
            {
                string modelName = modelInfos
                    .Where(x => x.Id == uniqueId)
                    .Select(x => x.ModelName).First();
                result.Add(modelName);
            }
            return result;
        }

        public List<Model> GetSelectedModels(Project project)
        {
            var result = new List<Model>();
            SelectionFilter filter = new SelectionFilter(true);

            //Get All Models as Model Id and name
            List<Model> models = project.ModelManager.GetAllModels()
                .ToList();

            //Get Unique model Ids
            List<ModelObject> modelObjects = project.ModelObjectManager
                .GetModelObjects(filter, Trimble.Connect.Desktop.API.Common.ObjectSelectionMode.HighestLevelAssembliesAndSystems)
                .ToList();
            List<string> uniqueIds = modelObjects
                .Select(x => x.ModelIdentifier)
                .Distinct()
                .ToList();
            foreach (string uniqueId in uniqueIds)
            {
                result.Add(models.FirstOrDefault(x => x.Identifier == uniqueId));
            }
            return result;
        }

        public void SetPosition(List<Model> models, TransformSet transformSet)
        {
            foreach(Model model in models)
            {
                var originalPlace = model.Placement;
                var xOriginal = originalPlace.PositionX;
                var yOriginal = originalPlace.PositionY;
                var zOriginal = originalPlace.Elevation;
                var rOriginal = originalPlace.RotationZ;

                double xValue = xOriginal;
                double yValue = yOriginal;
                double zValue = zOriginal;
                double rValue = rOriginal;

                //Set X
                if(transformSet.TransformX.DoTransform)
                {
                    xValue = transformSet.TransformX.TransformValue;
                }

                //Set Y
                if(transformSet.TransformY.DoTransform)
                {
                    yValue = transformSet.TransformY.TransformValue;
                }

                //Set Z
                if(transformSet.TransformZ.DoTransform)
                {
                    zValue = transformSet.TransformZ.TransformValue;
                }

                //Set R
                if(transformSet.TransformR.DoTransform)
                {
                    rValue = transformSet.TransformR.TransformValue;
                }


                model.SetPosition(xValue, yValue, zValue);
                model.SetRotation(rValue, Trimble.Connect.Desktop.API.Common.RotationAxis.Z);
            }
        }
    }

    public class ModelIdentifier
    {
        //Constructor
        public ModelIdentifier(string id, string modelName)
        {
            _id = id;
            _modelName = modelName;
        }

        //
        private string _id;
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _modelName;
        public string ModelName
        {
            get { return _modelName; }
            set { _modelName = value; }
        }
    }

    public class TransformArg
    {
        public TransformType Type { get; set; }
        public bool DoTransform { get; set; }
        public double TransformValue { get; set; }

        public enum TransformType
        {
            X,
            Y,
            Z,
            R
        }

        private TransformArg(TransformType type, bool doTransform, double value)
        {
            Type = type;
            DoTransform = doTransform;
            TransformValue = value;
        }

        public static TransformArg SetTransform(TransformType type, bool doTransform, string value)
        {
            double result;
            bool validTest = double.TryParse(value, out result);
            if(doTransform)
            {
                if (validTest)
                {
                    return new TransformArg(type, true, result);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new TransformArg(type, false, 0);
            }
        }
    }

    public class TransformSet
    {
        public TransformSet(TransformArg xArg, TransformArg yArg, TransformArg zArg, TransformArg rArg)
        {
            TransformX = xArg;
            TransformY = yArg;
            TransformZ = zArg;
            TransformR = rArg;
        }
        public TransformArg TransformX { get; set; }
        public TransformArg TransformY { get; set; }
        public TransformArg TransformZ { get; set; }
        public TransformArg TransformR { get; set; }
    }
}
