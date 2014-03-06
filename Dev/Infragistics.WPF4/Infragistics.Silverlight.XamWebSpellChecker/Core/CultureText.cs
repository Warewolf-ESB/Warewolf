using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Infragistics.Controls.Interactions;

namespace Infragistics.SpellChecker
	{
	/// <summary>Contains the text for the checker in different cultures</summary>
	internal class CultureText{

		static Dictionary<string,string> map;
		static LanguageType currentCultureMap;
		static bool firstRun = true;

		public static String GetCultureText(String key){
			return Get(key, LanguageType.English);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static String Get(String key, LanguageType culture){
			if (firstRun || currentCultureMap != culture) loadMap(culture);
			if(key==null) return "";
			String r = (String) map[key];
			if (r==null) return key;
			else return r;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		static void loadMap(LanguageType culture){
			map = new Dictionary<String,String>();
						
			if(culture == LanguageType.French){
				map.Add("Ignore", "Ignorer");
				map.Add("Ignore All", "Ignorer Tout");
				map.Add("Change", "Remplacer");
				map.Add("Change All", "Remp. Tout");
				map.Add("Add", "Ajouter");
				map.Add("Adding...", "s'ajouter..");
				map.Add("Cancel", "Annuler");
				map.Add("Finish", "Terminer");
				map.Add("Finished checking selection, would you like to check the rest of the text?", "Choix fini de v�rification.  V�rifiez le reste du texte?");
				map.Add("Finished Checking Selection", "Choix fini de v�rification.");
				map.Add("The spelling check is complete.", "Fini.");
				map.Add("The Spelling Check Is Complete.", "Fini.");
				map.Add("No Spelling Errors In Text.", "Aucunes Erreurs.");
				map.Add("Spelling", "Correcteur orthographique");
				map.Add("Check Spelling", "Correcteur Orthographique");
				map.Add("Checking Document...", "V�rifiant Le Document...");
				map.Add("Not In Dictionary:", "Non Trouv�");
				map.Add("Not in Dictionary:", "Non Trouv�");
				map.Add("In Dictionary:", "Trouv�");
				map.Add("Change To:", "Non Trouv�");
				map.Add("Resume", "Continuez");
				map.Add("Suggestions:", "Suggestions:");
				map.Add("Find Suggestions?", "Sugg�rer?");
				map.Add("Finding Suggestions...", "Recherche...");
				map.Add("No Suggestions.", "Aucun");
				map.Add("Spell checking document...", "Contr�le orthographique le texte...");

				map.Add("&Ignore", "&Ignorer");
				map.Add("Ignore &All", "Ignorer &Tout");
				map.Add("&Resume", "Continue&z");
				map.Add("A&dd", "&Ajouter");
				map.Add("&Change", "&Remplacer");
				map.Add("Chan&ge All", "Remp. T&out");
				map.Add("Canc&el", "Annul&er");


			} else if(culture == LanguageType.Spanish){
				map.Add("Ignore", "Ignorar");
				map.Add("Ignore All", "Ignorar Todas");
				map.Add("Change", "Cambiar");
				map.Add("Change All", "Cambiar Todos");
				map.Add("Add", "Agregar");
				map.Add("Adding...", "Adici�n...");
				map.Add("Cancel", "Cancelar");
				map.Add("Finish", "Acabar");
				map.Add("Finished checking selection, would you like to check the rest of the text?", "Selecci�n acabada.  �Verifique el resto del texto?");
				map.Add("Finished Checking Selection", "Selecci�n acabada.");
				map.Add("The spelling check is complete.", "La verificaci�n ortogr�fica ha finalizado.");
				map.Add("The Spelling Check Is Complete.", "La verificaci�n ortogr�fica ha finalizado.");
				map.Add("No Spelling Errors In Text.", "No se encontraron errores.");
				map.Add("Spelling", "Corregir Ortograf�a");
				map.Add("Check Spelling", "Corregir Ortograf�a");
				map.Add("Checking Document...", "Comprobaci�n Del Documento...");
				map.Add("Not in Dictionary:", "Palabra mal escrita");
				map.Add("Not In Dictionary:", "Palabra mal escrita");
				map.Add("In Dictionary:", "En Diccionario");
				map.Add("Change To:", "Palabra mal escrita");
				map.Add("Resume", "Contin�e");
				map.Add("Suggestions:", "Sugerencias:");
				map.Add("Find Suggestions?", "�Sugerencias?");
				map.Add("Finding Suggestions...", "Encontrar sugerencias...");
				map.Add("No Suggestions.", "Ningunas sugerencias");
				map.Add("Spell checking document...", "Comprobaci�n del deletreo el texto...");

				map.Add("&Ignore", "&Ignorar");
				map.Add("Ignore &All", "Ignorar &Todas");
				map.Add("&Resume", "C&ontin�e");
				map.Add("A&dd", "&Agregar");
				map.Add("&Change", "&Cambiar");
				map.Add("Chan&ge All", "Cambia&r Todos");
				map.Add("Canc&el", "Cance&lar");
			} else if(culture == LanguageType.German){
				map.Add("Ignore", "Ignorieren");
				map.Add("Ignore All", "Alle ignorieren");
				map.Add("Change", "�ndern");
				map.Add("Change All", "Alle �ndern");
				map.Add("Add", "Hinzuf�gen");
				map.Add("Adding...", "Hinzuf�gen...");
				map.Add("Cancel", "Abbrechen");
				map.Add("Finish", "Ende");
				map.Add("Finished checking selection, would you like to check the rest of the text?", "Der vorgew�hlte Text ist �berpr�ft worden. �berpr�fen Sie den Rest des Textes?");
				map.Add("Finished Checking Selection", "Der vorgew�hlte Text ist �berpr�ft worden.");
				map.Add("The spelling check is complete.", "Komplett.");
				map.Add("The Spelling Check Is Complete.", "Komplett.");
				map.Add("No Spelling Errors In Text.", "Keine Fehler.");
				map.Add("Spelling", "Rechtschreib�berpr�fung");
				map.Add("Check Spelling", "Rechtschreib�berpr�fung");
				map.Add("Checking Document...", "�berpr�fung des Dokumentes...");
				map.Add("Not in Dictionary:", "Nicht im W�rterbuch:");
				map.Add("Not In Dictionary:", "Nicht im W�rterbuch:");
				map.Add("In Dictionary:", "Im W�rterbuch:");
				map.Add("Change To:", "�ndern In:");
				map.Add("Resume", "Weiter");
				map.Add("Suggestions:", "Vorschl�ge:");
				map.Add("Find Suggestions?", "Vorschl�ge suchen");//"EntdeckungcVorschl�ge?");
				map.Add("Finding Suggestions...", "EntdeckungcVorschl�ge...");
				map.Add("No Suggestions.", "Keine Vorschl�ge");
				map.Add("Spell checking document...", "Bann�berpr�fungstext...");

				map.Add("&Ignore", "&Ignorieren");
				map.Add("Ignore &All", "&Alle ignorieren");
				map.Add("&Resume", "&Weiter");
				map.Add("A&dd", "&Hinzuf�gen");
				map.Add("&Change", "�&ndern");
				map.Add("Chan&ge All", "Alle �n&dern");
				map.Add("Canc&el", "Abbre&chen");
			} else if(culture == LanguageType.Italian){
				map.Add("Ignore", "Ignora");
				map.Add("Ignore All", "Ignora tutto");
				map.Add("Change", "Cambia in");
				map.Add("Change All", "Cambia tutto");
				map.Add("Add", "Aggiungi");
				map.Add("Adding...", "Aggiunta...");
				map.Add("Cancel", "Annullamento");
				map.Add("Finish", "Terminato");
				map.Add("Finished checking selection, would you like to check the rest of the text?", "Il testo selezionato � stato verificato.  Verifichi il resto del testo?");
				map.Add("Finished Checking Selection", "Il testo selezionato � stato verificato.");
				map.Add("The spelling check is complete.", "Completo.");
				map.Add("The Spelling Check Is Complete.", "Completo.");
				map.Add("No Spelling Errors In Text.", "Nessun errori nel testo.");
				map.Add("Spelling", "Verificatore ortografico");
				map.Add("Check Spelling", "Verificatore ortografico");
				map.Add("Checking Document...", "Controllo ortografico...");
				map.Add("Not in Dictionary:", "Cambia in:");
				map.Add("Not In Dictionary:", "Cambia in:");
				map.Add("In Dictionary:", "Approvazione:");
				map.Add("Change To:", "Cambia in:");
				map.Add("Resume", "Continui");
				map.Add("Suggestions:", "Suggerimenti:");
				map.Add("Find Suggestions?", "Suggerimenti?");
				map.Add("Finding Suggestions...", "Ottenere i suggerimenti...");
				map.Add("No Suggestions.", "Nessun suggerimenti");
				map.Add("Spell checking document...", "Controllo ortografico...");


				map.Add("&Ignore", "&Ignora");
				map.Add("Ignore &All", "Ignora &tutto");
				map.Add("&Resume", "C&ontinui");
				map.Add("A&dd", "&Aggiungi");
				map.Add("&Change", "&Cambia in");
				map.Add("Chan&ge All", "Ca&mbia tutto");
				map.Add("Canc&el", "A&nnullamento");
			} else if(culture == LanguageType.Portuguese){
				map.Add("Ignore", "Ignore");
				map.Add("Ignore All", "Ignore Tudo");
				map.Add("Change", "Mude");
				map.Add("Change All", "Mude Tudo");
				map.Add("Add", "Adicione");
				map.Add("Adding...", "Adi��o...");
				map.Add("Cancel", "Cancelamento");
				map.Add("Finish", "Batente");
				map.Add("Finished checking selection, would you like to check the rest of the text?", "A sele��o verificando terminada, voc� gosta de verificar o descanso do texto?");
				map.Add("Finished Checking Selection", "Sele��o Verificando Terminada");
				map.Add("The spelling check is complete.", "A verifica��o de soletra��o est� completa.");
				map.Add("The Spelling Check Is Complete.", "A Verifica��o De Soletra��o Est� Completa.");
				map.Add("No Spelling Errors In Text.", "Nenhuns Erros De Soletra��o No Texto.");
				map.Add("Spelling", "Soletra��o");
				map.Add("Check Spelling", "Verifique A Soletra��o");
				map.Add("Checking Document...", "Verificando O Original...");
				map.Add("Not in Dictionary:", "N�o no dicion�rio:");
				map.Add("Not In Dictionary:", "N�o No Dicion�rio:");
				map.Add("In Dictionary:", "No Dicion�rio:");
				map.Add("Change To:", "Mudan�a A:");
				map.Add("Resume", "Resumo");
				map.Add("Suggestions:", "Sugest�es:");
				map.Add("Find Suggestions?", "Sugest�es Do Achado?");
				map.Add("Finding Suggestions...", "Encontrando Sugest�es...");
				map.Add("No Suggestions.", "Nenhumas Sugest�es.");
				map.Add("Spell checking document...", "Original verificar de per�odo...");

				map.Add("&Ignore", "&Ignore");
				map.Add("Ignore &All", "Ignore &Tudo");
				map.Add("&Resume", "&Resumo");
				map.Add("A&dd", "&Adicione");
				map.Add("&Change", "&Mude");
				map.Add("Chan&ge All", "Mude T&udo");
				map.Add("Canc&el", "&Cancelamento");
			} else {									//default - English
				map.Add("Ignore", "Ignore");
				map.Add("Ignore All", "Ignore All");
				map.Add("Change", "Change");
				map.Add("Change All", "Change All");
				map.Add("Add", "Add");
				map.Add("Adding...", "Adding...");
				map.Add("Cancel", "Cancel");
				map.Add("Finish", "Finish");
				map.Add("Finished checking selection, would you like to check the rest of the text?", "Finished checking selection, would you like to check the rest of the text?");
				map.Add("Finished Checking Selection", "Finished Checking Selection");
				map.Add("The spelling check is complete.", "The spelling check is complete.");
				map.Add("The Spelling Check Is Complete.", "The Spelling Check Is Complete.");
				map.Add("No Spelling Errors In Text.", "No Spelling Errors In Text.");
				map.Add("Spelling", "Spelling");
				map.Add("Check Spelling", "Check Spelling");
				map.Add("Checking Document...", "Checking Document...");
				map.Add("Not in Dictionary:", "Not in Dictionary:");
				map.Add("Not In Dictionary:", "Not in Dictionary:");
				map.Add("In Dictionary:", "In Dictionary:");
				map.Add("Change To:", "Change To:");
				map.Add("Resume", "Resume");
				map.Add("Suggestions:", "Suggestions:");
				map.Add("Find Suggestions?", "Find Suggestions?");
				map.Add("Finding Suggestions...", "Finding Suggestions...");
				map.Add("No Suggestions.", "No Suggestions.");
				map.Add("Spell checking document...", "Spell checking document...");

				map.Add("&Ignore", "&Ignore");
				map.Add("Ignore &All", "Ignore &All");
				map.Add("&Resume", "&Resume");
				map.Add("A&dd", "A&dd");
				map.Add("&Change", "&Change");
				map.Add("Chan&ge All", "Chan&ge All");
				map.Add("Canc&el", "Canc&el");
			}

			currentCultureMap = culture;
			firstRun = false;
		}
	}

}
#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved