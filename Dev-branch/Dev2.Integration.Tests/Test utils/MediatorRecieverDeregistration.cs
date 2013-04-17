// FIX ME
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Dev2.Integration.Tests.Enums;
//using Dev2.Studio.Core;

//namespace Dev2.Integration.Tests.Test_utils {
//    public static class MediatorRecieverDeregistration {

//        public static void DeregisterForViewModel(enViewModelType viewModelToDeregisterFor) {
//            switch (viewModelToDeregisterFor){
//                case (enViewModelType.ConfigureDecisionViewModel) :
//                DeregisterConfigureDecisionViewModelMessages();
//                break;
//                case (enViewModelType.DataListItemViewModel) :
//                DeregisterDataListItemViewModelMessages();
//                break;
//                case (enViewModelType.DataListViewModel) :
//                DeregisterDataListViewModelMessages();
//                break;
//                default:
//                break;
//            }

//        }

//        public static void DeregisterForView(enViewTypes ViewType) {
//            switch (ViewType) {
//                case (enViewTypes.UserInterfaceLayoutProvider) :
//                DeregisterUserInterfaceLayoutProviderMessages();
//                break;
//                default:
//                break;
//            }
//        }

//        private static void DeregisterUserInterfaceLayoutProviderMessages() {
//            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.ShowStartPage);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddStuidoShortcutKeysPage);
//            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.ShowNavigation);
//            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.ShowConnectWindow);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddWorkflowDesigner);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.ShowWebpartWizard);
//            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.CloseWebpartWizard);
//            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddNewResource);
//            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.ShowResourcePropertyEditor);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddHelpDocument);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddWebpageDesigner);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddWebsiteDesigner);
//            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.BindWorkflowDesignXamlToViewModel);
//            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.ShowServiceInBrowser);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.SaveResourceModel);
//            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.DisconnectEnvironment);
//            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.ReconnectExistingEnvironment);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.AddResourceDocument);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.ShowDependencyGraph);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.ConfigureDecisionExpression);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.ConfigureSwitchExpression);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.ConfigureCaseExpression);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.WorkflowActivitySelected);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.RemoveDataMapping);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.TabContextChanged);
//           // Mediator.DeRegisterAllActionsForMessage(MediatorMessages.UpdateNavigationItemViewModel);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.UpdateWebpagePreview);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.UpdateResourceDesigner);
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.UpdateExplorer);
//            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.UpdateDeploy);
//        }

//        private static void DeregisterDataListViewModelMessages() {
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.UpdateDataList);
//        }

//        private static void DeregisterDataListItemViewModelMessages() {
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.DataListItemSelected);
//        }

//        private static void DeregisterConfigureDecisionViewModelMessages() {
//            Mediator.DeRegisterAllActionsForMessage(MediatorMessages.UpdateDataList);
//        }


//    }
//}
