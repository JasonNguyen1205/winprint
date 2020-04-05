﻿
using GalaSoft.MvvmLight.Ioc;
using WinPrint.Core.Services;

//using WinPrint.Services;
//using WinPrint.Views;

namespace WinPrint.Core.Models {
    public class ModelLocator {
        private static ModelLocator _current;

        public static ModelLocator Current => _current ?? (_current = new ModelLocator());

        private ModelLocator() {
            // Register the models via the Servcies Factory
            SimpleIoc.Default.Register<Settings>(SettingsService.Create);
            SimpleIoc.Default.Register<FileAssociations>(FileAssociationsService.Create);
            SimpleIoc.Default.Register<Options>();
        }

        public Models.Settings Settings => SimpleIoc.Default.GetInstance<Models.Settings>();

        public Models.Options Options => SimpleIoc.Default.GetInstance<Models.Options>();
        public Models.FileAssociations Associations => SimpleIoc.Default.GetInstance<Models.FileAssociations>();

        public void Register<VM, V>()
            where VM : class {
            SimpleIoc.Default.Register<VM>();
        }

        public static void Reset() {
            _current = null;
            SimpleIoc.Default.Unregister<Settings>();
            SimpleIoc.Default.Unregister<FileAssociations>();
            SimpleIoc.Default.Unregister<Options>();
        }
    }
}
