using System;
using System.ComponentModel;
using System.Windows.Input;
using DiagramDesigner.Annotations;
using DiagramDesigner.Model;
using DiagramDesigner.sNpFile;
using DiagramDesigner.ViewModel;
using DiagramDesigner.ViewModel.ViewModelBases;

namespace DiagramDesigner.BlockTypes.ImportedComponent
{
    public class ImportedComponentVM : ElementVM
    {
        public ImportedComponentVM([NotNull] Element importedComponentModel) :base(importedComponentModel)
        {
        }

        public ImportedComponentVM()
            : this(new ImportedComponentModel())
        {
        }

        public string FileName
        {
            get
            {
                if (Element is ImportedComponentModel importedComponentModel)
                    return importedComponentModel.FileName;
                else
                    throw new Exception("Model of ImportedComponentVM should be type of ImportedComponentModel");
            }
        }

        #region Import from Touchstone *.sNp file


        private ICommand _loadFromTouchstoneResult;
        [Browsable(false)]
        public ICommand LoadFromTouchstoneResult => _loadFromTouchstoneResult ?? (_loadFromTouchstoneResult = new RelayCommand(LoadFromTouchstoneResultExecute));

        private void LoadFromTouchstoneResultExecute(object obj)
        {
            if (obj is TouchstoneResult touchstoneResult && Element is ImportedComponentModel importedComponentModel)
            {
                importedComponentModel.SMatrices = touchstoneResult.SMatrices;
                importedComponentModel.Frequencies = touchstoneResult.Frequencies;
            }
            else
            {
                throw new Exception("Load argument is not a TouchStoneResult or Model of ImportedComponentVM is not a ImportedComponentModel");
            }
        }

        #endregion
    }
}