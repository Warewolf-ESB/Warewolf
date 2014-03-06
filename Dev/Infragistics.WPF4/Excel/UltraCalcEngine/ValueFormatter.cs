using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;









using System.Drawing;
using System.Windows.Forms;

  







using UltraCalcValue = Infragistics.Documents.Excel.CalcEngine.ExcelCalcValue;

namespace Infragistics.Documents.Excel.CalcEngine





{


#region Infragistics Source Cleanup (Region)


































































#endregion // Infragistics Source Cleanup (Region)


	
	
	// MD 5/24/11 - TFS75560
	internal class ValueFormatter
	{
		#region Constants

		private const int PositiveNumberSection = 0;
		private const int NegativeNumberSection = 1;
		private const int ZeroNumberSection = 2;
		private const int TextSection = 3;

		#endregion // Constants

		#region Member Variables

		private FormatSection[] formatSections;

		private Workbook workbook;


		#endregion // Member Variables

		#region Constructor

		public ValueFormatter(

			Workbook workbook,

			string format,
			// MD 4/6/12 - TFS101506
			CultureInfo culture
			)
		{

			this.workbook = workbook;


			// MD 4/6/12 - TFS101506
			//this.formatSections = this.ParseFormat(format);
			this.formatSections = this.ParseFormat(format, culture);
		}

		#endregion // Constructor

		#region Methods


		// MD 2/14/12 - 12.1 - Table Support
		#region FormatValue

		public string FormatValue(double? value, string text, object cellValue, int availablePixels, WorksheetCellFormatData cellFormat, out SectionType formattedAs)
		{
			GetTextWidthHelper getTextWidthHelper = null;
			if (cellFormat != null)
				getTextWidthHelper = new GetTextWidthHelper(cellFormat);

			try
			{
				bool useHashes;
				string formattedText = null;
				if (cellValue is ErrorValue)
				{
					formattedAs = SectionType.Text;
					formattedText = text;

					if (getTextWidthHelper == null)
						return formattedText;

					int width = getTextWidthHelper.GetWidth(null, formattedText);
					if (width <= availablePixels)
						return formattedText;

					useHashes = true;
				}
				else if (this.TryFormatValue(value, text, cellValue, getTextWidthHelper, availablePixels, out formattedText, out useHashes, out formattedAs))
				{
					return formattedText;
				}

				if (useHashes && getTextWidthHelper != null)
					return ValueFormatter.GetAllHashes(getTextWidthHelper, availablePixels);

				formattedAs = SectionType.Text;
				return text;
			}
			finally
			{
				if (getTextWidthHelper != null)
					getTextWidthHelper.Dispose();
			}
		}

		#endregion // FormatValue

		// MD 2/14/12 - 12.1 - Table Support
		#region GetAllHashes

		private static string GetAllHashes(GetTextWidthHelper getTextWidthHelper, int availablePixels)
		{
			// If nothing else works, display all hashes.
			const char HashChar = '#';
			int width = getTextWidthHelper.GetWidth(null, HashChar.ToString());
			int hashCount = availablePixels / width;
			return new string(HashChar, hashCount);
		}

		#endregion // GetAllHashes


		#region GetRepeatCharacterCount

		private static int GetRepeatCharacterCount(string format, ref int i, params char[] possibleValues)
		{
			int count = 1;
			while (i + count < format.Length)
			{
				char testChar = format[i + count];

				if (Array.IndexOf(possibleValues, testChar) >= 0)
					count++;
				else
					break;
			}

			i += count - 1;
			return count;
		}

		#endregion // GetRepeatCharacterCount


		// MD 2/27/12 - 12.1 - Table Support
		#region GetSectionType

		public SectionType GetSectionType(double value)
		{
			FormatSection sectionForNumber = this.formatSections[ValueFormatter.PositiveNumberSection];
			if (sectionForNumber.MeetsCriteria(value) == false)
			{
				FormatSection secondSection = this.formatSections[ValueFormatter.NegativeNumberSection];

				if (secondSection != null)
				{
					sectionForNumber = secondSection;
					if (sectionForNumber.MeetsCriteria(value) == false)
					{
						FormatSection defaultSection = this.formatSections[ValueFormatter.ZeroNumberSection];

						if (defaultSection != null)
						{
							sectionForNumber = defaultSection;
						}
						else
						{
							if (sectionForNumber.HasCustomComparison)
								return SectionType.Default;
						}
					}
				}
				else if (sectionForNumber.HasCustomComparison)
				{
					return SectionType.Text;
				}
			}

			return sectionForNumber.SectionType;
		}

		#endregion // GetSectionType


		#region ParseFormat

		// MD 4/6/12 - TFS101506
		//private FormatSection[] ParseFormat(string format)
		private FormatSection[] ParseFormat(string format, CultureInfo culture)
		{
			FormatSection[] formatSections = new FormatSection[4];

			int currentFormatIndex = 0;

			// MD 4/6/12 - TFS101506
			//FormatSection currentFormatSection = new FormatSection(this, currentFormatIndex);
			FormatSection currentFormatSection = new FormatSection(this, currentFormatIndex, culture);

			formatSections[currentFormatIndex] = currentFormatSection;

			bool insertSpaceForNextChar = false;
			bool nextCharacterIsEscaped = false;
			bool nextCharacterIsRepeated = false;

			bool isValid = true;

			for (int i = 0; i < format.Length && isValid; i++)
			{
				char c = format[i];

				try
				{
					if (insertSpaceForNextChar)
					{
						currentFormatSection.AddWellKnownPart(WellKnownPartType.Whitespace);
						insertSpaceForNextChar = false;
						continue;
					}
					else if (nextCharacterIsEscaped || nextCharacterIsRepeated)
					{
						Debug.Assert(nextCharacterIsEscaped != nextCharacterIsRepeated, "These should not both be true.");

						currentFormatSection.AddLiteralFormatPart(c, nextCharacterIsRepeated);
						nextCharacterIsEscaped = false;
						nextCharacterIsRepeated = false;
						continue;
					}

					switch (c)
					{
						#region General

						case ';':
							// If there are too many sections, bail out.
							if (++currentFormatIndex == 4)
								return null;

							// MD 4/6/12 - TFS101506
							//formatSections[currentFormatIndex] = currentFormatSection = new FormatSection(this, currentFormatIndex);
							formatSections[currentFormatIndex] = currentFormatSection = new FormatSection(this, currentFormatIndex, culture);
							break;

						case '_':
							insertSpaceForNextChar = true;
							break;

						case '*':
							nextCharacterIsRepeated = true;
							break;

						case '\\':
							nextCharacterIsEscaped = true;
							break;

						case 'g':
						case 'G':
							if (format.Substring(i).StartsWith("general", StringComparison.InvariantCultureIgnoreCase))
							{
								i += 6;
								currentFormatSection.AddWellKnownPart(WellKnownPartType.GeneralString);
							}
							// MD 3/19/12 - TFS105157
							// 'g', 'gg', and 'ggg' can also be used to give the era name.
							else
							{
								int gCount = ValueFormatter.GetRepeatCharacterCount(format, ref i, 'g');
								switch (gCount)
								{
									case 1:
										currentFormatSection.AddWellKnownPart(WellKnownPartType.EraEnglishName);
										break;

									case 2:
										currentFormatSection.AddWellKnownPart(WellKnownPartType.EraAbbreviatedName);
										break;

									default:
									case 3:
										currentFormatSection.AddWellKnownPart(WellKnownPartType.EraName);
										break;
								}
							}
							break;

						case '@':
							currentFormatSection.AddWellKnownPart(WellKnownPartType.VerbatimString);
							break;

						case '[':
							if (ValueFormatter.ParseFormatWithinBracket(currentFormatSection, format, ref i) == false)
								return null;

							break;

						case '/':
							switch (currentFormatSection.SectionType)
							{
								// The' /' cannot appear in the text section or a section whose type hasn't been determined yet, but it can appear 
								// in the date and number sections
								case SectionType.Default:
								case SectionType.Text:
									return null;

								case SectionType.Date:
									currentFormatSection.AddLiteralFormatPart(c);
									break;

								case SectionType.Number:
									currentFormatSection.AddWellKnownPart(WellKnownPartType.FractionSeparator);

									if (ValueFormatter.ParseFormatAfterFractionSeparator(currentFormatSection, format, ref i) == false)
										return null;

									break;

								default:
									Utilities.DebugFail("Unknown section type.");
									break;
							}
							break;

						case '"':
							{
								int startIndex = i + 1;

								bool foundCloseQuote = false;
								for (i = startIndex; i < format.Length; i++)
								{
									if (format[i] == '"')
									{
										foundCloseQuote = true;
										break;
									}
								}

								if (foundCloseQuote == false)
									return null;

								currentFormatSection.AddLiteralFormatPart(format.Substring(startIndex, i - startIndex));
							}
							break;

						#endregion // General

						#region Number Characters

						case '0':
							currentFormatSection.AddWellKnownPart(WellKnownPartType.DigitOrZero);
							break;

						case '#':
							currentFormatSection.AddWellKnownPart(WellKnownPartType.DigitOrEmpty);
							break;

						case '?':
							currentFormatSection.AddWellKnownPart(WellKnownPartType.DigitOrWhitespace);
							break;

						case '.':
							// The '.' cannot appear in the text section, but it can appear in the date and number sections
							if (currentFormatSection.SectionType == SectionType.Text)
								return null;

							if (currentFormatSection.SectionType == SectionType.Default)
							{
								currentFormatSection.SetSectionType(SectionType.Number);
							}
							else if (currentFormatSection.SectionType == SectionType.Date)
							{
								if (i < format.Length - 1 && format[i + 1] == '0')
								{
									i++;
									int zeroCount = ValueFormatter.GetRepeatCharacterCount(format, ref i, '0');
									switch (zeroCount)
									{
										case 1:
											currentFormatSection.AddWellKnownPart(WellKnownPartType.Milliseconds1);
											break;

										case 2:
											currentFormatSection.AddWellKnownPart(WellKnownPartType.Milliseconds2);
											break;

										case 3:
											currentFormatSection.AddWellKnownPart(WellKnownPartType.Milliseconds3);
											break;

										default:
											return null;
									}
								}
								else
								{
									currentFormatSection.AddLiteralFormatPart(c);
								}

								break;
							}

							currentFormatSection.AddWellKnownPart(WellKnownPartType.DecimalSeparator);
							break;

						case ',':
							{
								int commaCount = ValueFormatter.GetRepeatCharacterCount(format, ref i, ',');

								WellKnownPart wellKnownPart = currentFormatSection.LastPart as WellKnownPart;

								// If the comma follows a digit, it is a special character. Otherwise, it is just a literal character.
								if (wellKnownPart != null && WellKnownPart.IsDigit(wellKnownPart.Type))
								{
									char nextChar = (char)0;
									if (i < format.Length - 1)
										nextChar = format[i + 1];

									switch (nextChar)
									{
										case '0':
										case '#':
										case '?':
											// If we found a group separator after a scientific notation element, the format is invalid.
											if (currentFormatSection.HasScientificNotation)
												return null;

											// If we found a comma between two digit characters in the number portion (not the fractional portion), 
											// show group separators in the number portion.
											if (currentFormatSection.HasDecimalSeparator == false)
												currentFormatSection.ShowGroupSeparators();
											break;

										default:
											if (currentFormatSection.HasScientificNotation == false)
												currentFormatSection.AddWellKnownPart(WellKnownPartType.GroupShiftPlaceholder, commaCount);
											break;
									}
								}
								else
								{
									currentFormatSection.AddLiteralFormatPart(c);
								}
							}
							break;

						case '%':
							currentFormatSection.AddWellKnownPart(WellKnownPartType.Percent);
							break;

						#endregion // Number Characters

						#region DateTime Characters

						case 'b':
							{
								int bCount = ValueFormatter.GetRepeatCharacterCount(format, ref i, 'b');

								if (bCount <= 2)
									currentFormatSection.AddWellKnownPart(WellKnownPartType.BuddhistYearsShort);
								else
									currentFormatSection.AddWellKnownPart(WellKnownPartType.BuddhistYearsLong);
							}
							break;

						case 'e':
							// MD 3/19/12 - TFS105157
							// 'e' and 'ee' are the year in the era if a calendar with eras is used.
							//
							//// We will only use the first 'e', so skip past all the others.
							//int eCount = ValueFormatter.GetRepeatCharacterCount(format, ref i, 'e');
							//currentFormatSection.AddWellKnownPart(WellKnownPartType.YearsLong);
							int eCount = ValueFormatter.GetRepeatCharacterCount(format, ref i, 'e');
							if (eCount == 1)
								currentFormatSection.AddWellKnownPart(WellKnownPartType.EraYearsShort);
							else
								currentFormatSection.AddWellKnownPart(WellKnownPartType.EraYearsLong);

							break;

						case 'y':
						case 'Y':
							{
								int yCount = ValueFormatter.GetRepeatCharacterCount(format, ref i, 'y', 'Y');

								if (yCount <= 2)
									currentFormatSection.AddWellKnownPart(WellKnownPartType.YearsShort);
								else
									currentFormatSection.AddWellKnownPart(WellKnownPartType.YearsLong);
							}
							break;

						case 'm':
						case 'M':
							{
								int mCount = ValueFormatter.GetRepeatCharacterCount(format, ref i, 'm', 'M');

								WellKnownPart lastDateTimePart = currentFormatSection.GetLastDateTimePart();

								if (mCount <= 2 && lastDateTimePart != null && WellKnownPart.IsHour(lastDateTimePart.Type))
								{
									switch (mCount)
									{
										case 1:
											currentFormatSection.AddWellKnownPart(WellKnownPartType.MinutesShort);
											break;

										case 2:
											currentFormatSection.AddWellKnownPart(WellKnownPartType.MinutesLong);
											break;
									}
								}
								else
								{
									switch (mCount)
									{
										case 1:
											currentFormatSection.AddWellKnownPart(WellKnownPartType.MonthsShort);
											break;

										case 2:
											currentFormatSection.AddWellKnownPart(WellKnownPartType.MonthsLong);
											break;

										case 3:
											currentFormatSection.AddWellKnownPart(WellKnownPartType.MonthShortDescription);
											break;

										default:
										case 4:
											currentFormatSection.AddWellKnownPart(WellKnownPartType.MonthLongDescription);
											break;

										case 5:
											currentFormatSection.AddWellKnownPart(WellKnownPartType.MonthFirstLetter);
											break;
									}
								}
							}
							break;

						case 'd':
						case 'D':
							{
								int dCount = ValueFormatter.GetRepeatCharacterCount(format, ref i, 'd', 'D');

								switch (dCount)
								{
									case 1:
										currentFormatSection.AddWellKnownPart(WellKnownPartType.DaysShort);
										break;

									case 2:
										currentFormatSection.AddWellKnownPart(WellKnownPartType.DaysLong);
										break;

									case 3:
										currentFormatSection.AddWellKnownPart(WellKnownPartType.DayShortDescription);
										break;

									default:
									case 4:
										currentFormatSection.AddWellKnownPart(WellKnownPartType.DayLongDescription);
										break;
								}
							}
							break;

						case 'h':
						case 'H':
							{
								int hCount = ValueFormatter.GetRepeatCharacterCount(format, ref i, 'h', 'H');

								switch (hCount)
								{
									case 1:
										currentFormatSection.AddWellKnownPart(WellKnownPartType.HoursShort);
										break;

									default:
									case 2:
										currentFormatSection.AddWellKnownPart(WellKnownPartType.HoursLong);
										break;
								}
							}
							break;

						case 's':
						case 'S':
							{
								int sCount = ValueFormatter.GetRepeatCharacterCount(format, ref i, 's', 'S');

								WellKnownPart lastDateTimePart = currentFormatSection.GetLastDateTimePart();

								// If there was a month part before the seconds, convert it to a minutes part instead.
								if (lastDateTimePart != null)
									lastDateTimePart.ConvertMonthPartToMinutePart();

								switch (sCount)
								{
									case 1:
										currentFormatSection.AddWellKnownPart(WellKnownPartType.SecondsShort);
										break;

									default:
									case 2:
										currentFormatSection.AddWellKnownPart(WellKnownPartType.SecondsLong);
										break;
								}
							}
							break;

						case 'a':
						case 'A':
							{
								if (format.Length - i >= 3)
								{
									string apSection = format.Substring(i, 3);

									if (apSection.Equals("a/p", StringComparison.InvariantCultureIgnoreCase))
									{
										i += 2;

										bool lowerAM = apSection[0] == 'a';
										bool lowerPM = apSection[2] == 'p';

										WellKnownPartType ampmShortType = lowerAM
											? (ampmShortType = lowerPM ? WellKnownPartType.AMPMShortLL : WellKnownPartType.AMPMShortLU)
											: (ampmShortType = lowerPM ? WellKnownPartType.AMPMShortUL : WellKnownPartType.AMPMShortUU);

										currentFormatSection.AddWellKnownPart(ampmShortType);
										break;
									}
								}

								if (format.Length - i >= 5)
								{
									string ampmSection = format.Substring(i, 5);

									if (ampmSection.Equals("am/pm", StringComparison.InvariantCultureIgnoreCase))
									{
										i += 4;
										currentFormatSection.AddWellKnownPart(WellKnownPartType.AMPMLong);
										break;
									}
								}

								currentFormatSection.AddLiteralFormatPart(c);
							}
							break;

						// MD 3/24/12 - TFS105636
						// Added support for the Chinese AM/PM format code.
						case '上':
							{
								if (format.Length - i >= 5)
								{
									string ampmSection = format.Substring(i, 5);

									if (ampmSection.Equals("上午/下午", StringComparison.InvariantCultureIgnoreCase))
									{
										i += 4;
										currentFormatSection.AddWellKnownPart(WellKnownPartType.AMPMChinese);
										break;
									}
								}

								currentFormatSection.AddLiteralFormatPart(c);
							}
							break;

						#endregion // DateTime Characters

						case 'E':
							if (++i < format.Length)
							{
								char sign = format[i];
								switch (sign)
								{
									case '-':
										currentFormatSection.AddWellKnownPart(WellKnownPartType.Scientific);
										break;

									case '+':
										currentFormatSection.AddWellKnownPart(WellKnownPartType.ScientificWithSign);
										break;

									default:
										return null;
								}
							}
							else
							{
								return null;
							}

							break;

						case 'B':
						case 'N':
						case 'n':
							return null;

						default:
							currentFormatSection.AddLiteralFormatPart(c);
							break;
					}
				}
				finally
				{
					isValid = currentFormatSection.IsValid;
				}
			}

			if (isValid == false)
				return null;

			if (insertSpaceForNextChar || nextCharacterIsEscaped || nextCharacterIsRepeated)
				return null;

			// If there are more than one sections...
			if (formatSections[1] != null)
			{
				if (formatSections[ValueFormatter.PositiveNumberSection].HasVerbatimString)
					return null;

				// MD 2/14/12 - 12.1 - Table Support
				// We also need to change the condition types for some of the sections depending on what other sections are available.
				//if (formatSections[ValueFormatter.NegativeNumberSection].HasVerbatimString &&
				//    formatSections[ValueFormatter.ZeroNumberSection] != null)
				//    return null;
				//
				//if (formatSections[ValueFormatter.ZeroNumberSection] != null &&
				//    formatSections[ValueFormatter.ZeroNumberSection].HasVerbatimString &&
				//    formatSections[ValueFormatter.TextSection] != null)
				//    return null;
				if (formatSections[ValueFormatter.NegativeNumberSection].HasVerbatimString)
				{
					if (formatSections[ValueFormatter.ZeroNumberSection] != null)
						return null;

					// If the second section has a verbatim string ('@') and the first section doesn't have an explicit condition, it can accept any value.
					if (formatSections[ValueFormatter.PositiveNumberSection].HasCustomComparison == false)
						formatSections[ValueFormatter.PositiveNumberSection].SetComparisonType(CompareOperator.AnyValue, 0, false);
				}
				else if (formatSections[ValueFormatter.ZeroNumberSection] == null)
				{
					// If there are only two sections and the first section doesn't have an explicit condition, it can accept the zero value as well as positive numbers.
					if (formatSections[ValueFormatter.PositiveNumberSection].HasCustomComparison == false)
						formatSections[ValueFormatter.PositiveNumberSection].SetComparisonType(CompareOperator.GreaterThanOrEquals, 0, false);
				}

				if (formatSections[ValueFormatter.ZeroNumberSection] != null &&
					formatSections[ValueFormatter.ZeroNumberSection].HasVerbatimString)
				{
					if (formatSections[ValueFormatter.TextSection] != null)
						return null;

					// If the third section has a verbatim string ('@') and the second section doesn't have an explicit condition, it can accept any value.
					if (formatSections[ValueFormatter.NegativeNumberSection].HasCustomComparison == false)
						formatSections[ValueFormatter.NegativeNumberSection].SetComparisonType(CompareOperator.AnyValue, 0, false);
				}

				FormatSection textSection = formatSections[ValueFormatter.TextSection];

				if (textSection != null &&
					textSection.SectionType != SectionType.Text && textSection.SectionType != SectionType.Default)
				{
					return null;
				}
			}

			return formatSections;
		}

		#endregion // ParseFormat

		#region ParseFormatAfterFractionSeparator

		private static bool ParseFormatAfterFractionSeparator(FormatSection currentFormatSection, string format, ref int i)
		{
			string denominatorText = string.Empty;
			bool denominatorTextContains1Thru9Digit = false;

			bool isDenominatorPortionStarted = false;
			bool isDenominatorPortionCompleted = false;
			bool foundALiteralOrWhitespace = false;

			bool insertSpaceForNextChar = false;
			bool nextCharacterIsEscaped = false;
			bool nextCharacterIsRepeated = false;

			for (i++; i < format.Length; i++)
			{
				char c = format[i];

				if (insertSpaceForNextChar)
				{
					if (isDenominatorPortionStarted)
						isDenominatorPortionCompleted = true;

					foundALiteralOrWhitespace = true;
				    currentFormatSection.AddWellKnownPart(WellKnownPartType.Whitespace);
				    insertSpaceForNextChar = false;
				    continue;
				}
				else if (nextCharacterIsEscaped || nextCharacterIsRepeated)
				{
				    Debug.Assert(nextCharacterIsEscaped != nextCharacterIsRepeated, "These should not both be true.");

					if (isDenominatorPortionStarted)
						isDenominatorPortionCompleted = true;

					foundALiteralOrWhitespace = true;
				    currentFormatSection.AddLiteralFormatPart(c, nextCharacterIsRepeated);
				    nextCharacterIsEscaped = false;
				    nextCharacterIsRepeated = false;
				    continue;
				}

				if (c == ';')
				{
					i--;
					break;
				}

				switch (c)
				{
					case '_':
						insertSpaceForNextChar = true;
						break;

					case '*':
						nextCharacterIsRepeated = true;
						break;

					case '\\':
						nextCharacterIsEscaped = true;
						break;

					case '#':
						if (isDenominatorPortionCompleted)
						{
							currentFormatSection.AddLiteralFormatPart(0.ToString());
						}
						else
						{
							currentFormatSection.AddWellKnownPart(WellKnownPartType.DigitOrEmpty);
							isDenominatorPortionStarted = true;
							denominatorText += c;
						}
						break;

					case '?':
						if (isDenominatorPortionCompleted)
						{
							currentFormatSection.AddLiteralFormatPart(' ');
						}
						else
						{
							currentFormatSection.AddWellKnownPart(WellKnownPartType.DigitOrWhitespace);
							isDenominatorPortionStarted = true;
							denominatorText += c;
						}
						break;

					case '0':
						if (isDenominatorPortionCompleted)
						{
							currentFormatSection.AddLiteralFormatPart(c);
						}
						else
						{
							currentFormatSection.AddWellKnownPart(WellKnownPartType.DigitOrZero);
							isDenominatorPortionStarted = true;
							denominatorText += c;
						}
						break;

					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						if (isDenominatorPortionCompleted)
						{
							currentFormatSection.AddLiteralFormatPart(c);
						}
						else
						{
							currentFormatSection.AddWellKnownPart(WellKnownPartType.DigitOrZero);
							isDenominatorPortionStarted = true;
							denominatorText += c;
							denominatorTextContains1Thru9Digit = true;
						}
						break;

						// The dot is not allowed to be right after the '/' or right after a group of digits right after the '/' or 
						// before the first group of digits after the '/'. But if there is a denominator and at least one literal after 
						// the denominator, a dot is allowed.
					case '.':
						{
							if (foundALiteralOrWhitespace == false || isDenominatorPortionCompleted == false)
								return false;

							goto default;
						}

						// The comma is not allowed to be right after the '/' or right after a digit or dot.
						// Otherwise, it is allowed.
					case ',':
						{
							Part lastPart = currentFormatSection.LastPart;
							WellKnownPart lastWellKnownPart = lastPart as WellKnownPart;
							if (lastWellKnownPart != null && WellKnownPart.IsDigit(lastWellKnownPart.Type))
								return false;

							LiteralFormatPart literalPart = lastPart as LiteralFormatPart;
							if (literalPart != null && literalPart.Text == ".")
								return false;

							goto default;
						}

					#region Not Allowed After '/'

					case '@':
					case '/':
					case 'b':
					case 'B':
					case 'd':
					case 'D':
					case 'e':
					case 'E':
					case 'g':
					case 'G':
					case 'h':
					case 'H':
					case 'm':
					case 'M':
					case 'n':
					case 'N':
					case 's':
					case 'S':

					// MD 2/8/12 - 12.1 - Table Support
					// Let these fall through to the default case. After a fraction separator, the 't' and 'T' format characters are 
					// treated as literals by Excel.
					//case 't':
					//case 'T':

					case 'y':
					case 'Y':
						return false;

					#endregion // Not Allowed After '/'

					default:
						if (isDenominatorPortionStarted)
							isDenominatorPortionCompleted = true;

						foundALiteralOrWhitespace = true;
						currentFormatSection.AddLiteralFormatPart(c);
						break;
				}
			}

			if (insertSpaceForNextChar || nextCharacterIsEscaped || nextCharacterIsRepeated)
				return false;

			if (denominatorText.Length == 0)
				return false;

			int denominator;
			if (int.TryParse(denominatorText, out denominator) == false && denominatorTextContains1Thru9Digit)
				return false;

			if (denominator == 0)
				currentFormatSection.GeneratedDenominatorMaxDigitCount = denominatorText.Length;
			else
				currentFormatSection.GeneratedDenominatorSpecifiedValue = denominator;

			return true;
		}

		#endregion // ParseFormatAfterFractionSeparator

		#region ParseFormatWithinBracket

		private static bool ParseFormatWithinBracket(FormatSection currentFormatSection, string format, ref int i)
		{
			i++;

			int firstCharacterInBracketIndex = i;
			string stringFromBracket = format.Substring(firstCharacterInBracketIndex);
			char firstCharacterInBracket = format[firstCharacterInBracketIndex];

			switch (firstCharacterInBracket)
			{
				case 'h':
				case 'H':
					{
						int hCount = ValueFormatter.GetRepeatCharacterCount(format, ref i, 'h', 'H');
						int whiteSpaceAfterHValues = ValueFormatter.GetRepeatCharacterCount(format, ref i, ' ');

						if (format.Length <= ++i || format[i] != ']')
							return false;

						currentFormatSection.AddWellKnownPart(WellKnownPartType.HoursTotal, hCount);
						return true;
					}

				case 's':
				case 'S':
					{
						int sCount = ValueFormatter.GetRepeatCharacterCount(format, ref i, 's', 'S');
						int whiteSpaceAfterSValues = ValueFormatter.GetRepeatCharacterCount(format, ref i, ' ');

						if (format.Length <= ++i || format[i] != ']')
							return false;

						currentFormatSection.AddWellKnownPart(WellKnownPartType.SecondsTotal, sCount);
						return true;
					}

				case 'm':
				case 'M':
					{
						int mCount = ValueFormatter.GetRepeatCharacterCount(format, ref i, 'm', 'M');
						int whiteSpaceAfterMValues = ValueFormatter.GetRepeatCharacterCount(format, ref i, ' ');

						if (format.Length <= ++i)
							return false;

						char nextChar = format[i];

						switch (format[i])
						{
							case ']':
								currentFormatSection.AddWellKnownPart(WellKnownPartType.MinutesTotal, mCount);
								return true;

							case 'a':
							case 'A':
								return ValueFormatter.TestForBracketedColor(currentFormatSection, format, firstCharacterInBracketIndex, stringFromBracket, ref i, "magenta");
						}

						return false;
					}

				case 'c':
				case 'C':	
					if (stringFromBracket.StartsWith("color", StringComparison.InvariantCultureIgnoreCase))
					{
						i = firstCharacterInBracketIndex + 5;
						double index;
						if (ValueFormatter.ParseNumberAfterBracketedOperator(format, true, ref i, out index) == false)
							return false;

						if (index < 1 || 56 < index)
							return false;

						if (currentFormatSection.SetColor(index.ToString()) == false)
							return false;

						return true;
					}

					return ValueFormatter.TestForBracketedColor(currentFormatSection, format, firstCharacterInBracketIndex, stringFromBracket, ref i, "cyan");

				case '$':
					{
						bool foundDash = false;
						for (i++; i < format.Length; i++)
						{
							char c = format[i];

							if (c == '-')
							{
								foundDash = true;
								break;
							}

							if (c == ']')
								return true;

							currentFormatSection.AddLiteralFormatPart(c);
						}

						if (foundDash == false)
							return false;

						string hexNumberText = string.Empty;

						for (i++; i < format.Length; i++)
						{
							char c = format[i];

							if (c == ' ')
							{
								// If there are spaces between the '-' and number, skip past them.
								if (hexNumberText.Length == 0)
								{
									ValueFormatter.GetRepeatCharacterCount(format, ref i, ' ');
									continue;
								}
								
								break;
							}

							if ('0' <= c && c <= '9' ||
								'a' <= c && c <= 'f' ||
								'A' <= c && c <= 'F')
							{
								hexNumberText += c;
							}
							else
							{
								break;
							}
						}

						if (i < format.Length && format[i] == ']')
						{
							int localeId;
							if (int.TryParse(hexNumberText, NumberStyles.HexNumber, currentFormatSection.Culture, out localeId) == false)
								return true;

							if (currentFormatSection.SetLocaleId(localeId) == false)
								return false;

							// If we have a forced date or time code, skip everything else in the section.
							if (currentFormatSection.IsForcedDateTimeStyle)
							{
								for (i++; i < format.Length; i++)
								{
									if (format[i] == ';')
									{
										i--;
										break;
									}
								}
							}

							return true;
						}

						for (; i < format.Length; i++)
						{
							if (format[i] == ']')
								return true;
						}

						return false;
					}

				case '=':
				case '<':
				case '>':
					{
						if (stringFromBracket.Length < 2)
							return false;

						CompareOperator compareOperator;

						string firstTwoLetters = stringFromBracket.Substring(0, 2);
						switch (firstTwoLetters)
						{
							case "<>":
								i += 2;
								compareOperator = CompareOperator.NotEquals;
								break;

							case "<=":
								i += 2;
								compareOperator = CompareOperator.LessThanOrEquals;
								break;

							case ">=":
								i += 2;
								compareOperator = CompareOperator.GreaterThanOrEquals;
								break;

							default:
								{
									switch (firstCharacterInBracket)
									{
										case '=':
											compareOperator = CompareOperator.Equals;
											break;

										case '<':
											compareOperator = CompareOperator.LessThan;
											break;

										case '>':
											compareOperator = CompareOperator.GreaterThan;
											break;

										default:
											Utilities.DebugFail("This was not expected.");
											return false;
									}

									i++;
								}
								break;
						}

						double compareOperand;
						ValueFormatter.ParseNumberAfterBracketedOperator(format, false, ref i, out compareOperand);

						if (currentFormatSection.SetComparisonType(compareOperator, compareOperand) == false)
							return false;

						for (; i < format.Length; i++)
						{
							if (format[i] == ']')
								return true;
						}

						return false;
					}

				default:
					return ValueFormatter.TestForBracketedColor(currentFormatSection, format, firstCharacterInBracketIndex, stringFromBracket, ref i, "blue", "black", "green", "red", "white", "yellow");
			}
		}

		#endregion // ParseFormatWithinBracket

		#region ParseNumberAfterBracketedOperator

		private static bool ParseNumberAfterBracketedOperator(string format, bool isForColorIndex, ref int i, out double number)
		{
			number = 0;

			if (isForColorIndex)
			{
				if (i == format.Length)
					return false;

				if (format[i] == ' ')
				{
					ValueFormatter.GetRepeatCharacterCount(format, ref i, ' ');
					i++;
				}
			}

			string numberText = string.Empty;
			for (; i < format.Length; i++)
			{
				char c = format[i];

				if (c == ']')
					break;

				if (c == '-' || c == '+')
				{
					if (isForColorIndex)
						return false;

					if (numberText.Length != 0)
						return false;

					numberText += c;
					continue;
				}

				if ('0' <= c && c <= '9')
				{
					numberText += c;
					continue;
				}

				if (c == '.')
				{
					numberText += c;
					continue;
				}

				break;
			}

			if (numberText.Length == 0)
				return false;

			// MD 4/9/12 - TFS101506
			//if (double.TryParse(numberText, NumberStyles.Float, CultureInfo.InvariantCulture, out number) == false)
			if (MathUtilities.DoubleTryParse(numberText, CultureInfo.InvariantCulture, out number) == false)
				return false;

			for (; i < format.Length; i++)
			{
				if (format[i] == ']')
					return true;
			}

			return false;
		}

		#endregion // ParseNumberAfterBracketedOperator

		#region Round

		private static double Round(double value)
		{
			return ValueFormatter.Round(value, 0);
		}

		private static double Round(double value, int decimals)
		{






			return Math.Round(value, decimals, MidpointRounding.AwayFromZero);

		}

		#endregion // Round

		#region TestForBracketedColor

		private static bool TestForBracketedColor(FormatSection currentFormatSection, string format, int firstCharacterInBracketIndex, string stringFromBracket, ref int i, params string[] colors)
		{
			foreach (string color in colors)
			{
				if (stringFromBracket.StartsWith(color, StringComparison.InvariantCultureIgnoreCase) == false)
					continue;

				if (currentFormatSection.SetColor(color) == false)
					return false;

				for (i = firstCharacterInBracketIndex + color.Length; i < format.Length; i++)
				{
					if (format[i] == ']')
						return true;
				}
			}

			return false;
		}

		#endregion // TestForBracketedColor

		#region Truncate

		private static double Truncate(double value)
		{






			return Math.Truncate(value);

		}

		#endregion // Truncate

		#region TryFormatValue

		public bool TryFormatValue(double? value, string text, out string formattedText)
		{

			// MD 2/14/12 - 12.1 - Table Support
			// Moved all code to a new overload of the TryFormatValue method.
			bool useHashes;
			SectionType formattedAs;
			bool result = this.TryFormatValue(value, text, null, null, -1, out formattedText, out useHashes, out formattedAs);
			return result;
		}

		// MD 2/14/12 - 12.1 - Table Support
		// Added a new overload which takes the information needed to measure and constrain the text.
		private bool TryFormatValue(double? value, string text, object cellValue, GetTextWidthHelper getTextWidthHelper, int availablePixels, 
			out string formattedText, out bool useHashes, out SectionType formattedAs)
		{
			// MD 2/14/12 - 12.1 - Table Support
			// There seems to be a one off bug with the False value in MS Excel.
			if (cellValue is bool && (bool)cellValue == false)
				availablePixels--;

			// MD 2/14/12 - 12.1 - Table Support
			useHashes = false;
			formattedAs = SectionType.Text;


			formattedText = null;

			if (this.IsValid == false)
				return false;

			if (value.HasValue == false)
			{
				for (int i = this.formatSections.Length - 1; i >= 0; i--)
				{
					FormatSection section = this.formatSections[i];

					if (section == null)
						continue;

					if (section.SectionType == SectionType.Date || section.SectionType == SectionType.Number)
					{

						// MD 2/14/12 - 12.1 - Table Support
						// When a Boolean is used in a number or date section and it cannot fit, it is displayed as hashes, unlike text which 
						// will always display the full text.
						if (cellValue is bool)
						{
							int width = getTextWidthHelper.GetWidth(section, text);
							if (availablePixels < width)
							{
								useHashes = true;
								return false;
							}
						} 


						formattedText = text;
						return true;
					}

					// MD 2/14/12 - 12.1 - Table Support
					// FormatSection.FormatText now takes parameters needed to measure and constrain the text.
					//formattedText = section.FormatText(text);
					formattedText = section.FormatText(text

						, cellValue, getTextWidthHelper, availablePixels, out useHashes

						);

					return formattedText != null;
				}

				Utilities.DebugFail("This shouldn't have happened");
				return false;
			}


			// MD 2/14/12 - 12.1 - Table Support
			// All numeric values use hashes when they cannot be displayed.
			useHashes = true;
			formattedAs = SectionType.Number;


			// MD 3/2/12
			// Found while fixing TFS103665
			// We should use the rounded display value for getting the formatted text.
			//double valueResolved = value.Value;
			double valueResolved = MathUtilities.RoundToExcelDisplayValue(value.Value);

			FormatSection sectionForNumber = this.formatSections[ValueFormatter.PositiveNumberSection];
			if (sectionForNumber.MeetsCriteria(valueResolved) == false)
			{
				FormatSection secondSection = this.formatSections[ValueFormatter.NegativeNumberSection];

				if (secondSection != null)
				{
					sectionForNumber = secondSection;
					if (sectionForNumber.MeetsCriteria(valueResolved) == false)
					{
						FormatSection defaultSection = this.formatSections[ValueFormatter.ZeroNumberSection];

						if (defaultSection != null)
						{
							sectionForNumber = defaultSection;
						}
						else
						{
							if (sectionForNumber.HasCustomComparison)
								return false;
						}
					}
				}
				else if (sectionForNumber.HasCustomComparison)
				{
					formattedText = valueResolved.ToString();
					return true;
				}
			}

			// MD 2/14/12 - 12.1 - Table Support
			// FormatSection.FormatValue now takes parameters needed to measure and constrain the text.
			// Also, if it indicates that we should use a rounded value, round the decimal portion off and call recursively into this method.
			//formattedText = sectionForNumber.FormatValue(valueResolved);
			bool useRoundedValue = false;
			formattedText = sectionForNumber.FormatValue(valueResolved

				, getTextWidthHelper, availablePixels, out useRoundedValue, out formattedAs

				);

			if (formattedText == null && useRoundedValue)
			{
				return TryFormatValue(MathUtilities.MidpointRoundingAwayFromZero(valueResolved), text

					, cellValue, getTextWidthHelper, availablePixels 

					, out formattedText

					, out useHashes, out formattedAs

					);
			}

			return formattedText != null;
		}

		#endregion // TryFormatValue

		#endregion // Methods

		#region Properties

		#region IsValid

		public bool IsValid
		{
			get { return this.formatSections != null; }
		}

		#endregion // IsValid

		#endregion // Properties


		#region FormatSection class

		private class FormatSection
		{
			#region Member Variables

			private int cachedFractionDigitCount = -1;
			private int cachedGeneratedDenominatorDigitCount = -1;
			private int cachedGeneratedNumeratorDigitCount = -1;
			private int cachedNumberDigitCount = -1;
			private int cachedScientificExponentDigitCount = -1;

			// MD 3/19/12 - TFS105157
			private Calendar calendar;
			private Calendar calendarForEras;

			private string color;
			private double compareOperand;
			private CompareOperator compareOperator;
			private CultureInfo culture;
			private CultureInfo cultureForDates;
			private bool forcedLongDateStyle;
			private bool forcedLongTimeStyle;
			private ValueFormatter formatter;
			private int? generatedDenominatorSpecifiedValue;
			private int? generatedDenominatorMaxDigitCount;
			private int? generatedNumeratorPartIndex;

			// MD 3/16/12 - TFS105094
			private bool generatedFractionIsOptional;

			private bool generatedFractionUsesFullNumber = true;
			private int groupShiftCount;
			private bool hasAMPMPart;
			private bool hasCustomComparison;
			private bool hasDecimalSeparator;
			private bool hasFractionSeparator;

			// MD 2/8/12 - 12.1 - Table Support
			private bool hasMillisecondPart;

			private bool hasScientificNotation;
			private bool hasVerbatimString;
			private bool isValid = true;
			private List<Part> parts = new List<Part>();
			private int percentCount;
			private int sectionIndex;
			private SectionType sectionType;

			// MD 3/19/12 - TFS105157
			private bool shouldConvertNativeDigits;

			private bool shouldShowGroupSeparators;

			#endregion // Member Variables

			#region Constructor

			// MD 4/6/12 - TFS101506
			//public FormatSection(ValueFormatter formatter, int sectionIndex)
			public FormatSection(ValueFormatter formatter, int sectionIndex, CultureInfo culture)
			{
				// MD 4/6/12 - TFS101506
				//this.culture = CultureInfo.CurrentCulture;
				this.culture = culture;

				this.formatter = formatter;
				this.sectionIndex = sectionIndex;

				switch (this.sectionIndex)
				{
					case ValueFormatter.PositiveNumberSection:
						this.compareOperator = CompareOperator.GreaterThan;
						this.compareOperand = 0;
						break;

					case ValueFormatter.NegativeNumberSection:
						this.compareOperator = CompareOperator.LessThan;
						this.compareOperand = 0;
						break;
				}
			}

			#endregion // Constructor

			#region Methods

			#region AddLiteralFormatPart

			public void AddLiteralFormatPart(char c)
			{
				this.AddLiteralFormatPart(c, false);
			}

			public void AddLiteralFormatPart(char c, bool isRepeated)
			{
				this.parts.Add(new LiteralFormatPart(c.ToString(), isRepeated));
			}

			public void AddLiteralFormatPart(string text)
			{
				this.parts.Add(new LiteralFormatPart(text, false));
			}

			#endregion // AddLiteralFormatPart


			// MD 2/14/12 - 12.1 - Table Support
			#region AddPaddingCharacters

			private void AddPaddingCharacters(GetTextWidthHelper getTextWidthHelper, int availablePixels, int paddingPosition, char paddingChar, ref string formattedText, ref int width)
			{
				if (paddingChar == 0 || paddingPosition < 0)
					return;

				int extraWidth = availablePixels - width;

				string textBeforePadding = formattedText.Substring(0, paddingPosition);
				string textAfterPadding = formattedText.Substring(paddingPosition);

				int paddingTextWidth = getTextWidthHelper.GetWidth(this, paddingChar.ToString());
				int paddingCharCount = extraWidth / paddingTextWidth;

				if (paddingCharCount > 0)
					formattedText = textBeforePadding + new string(paddingChar, paddingCharCount) + textAfterPadding;

				#region Debug-Only Verification



#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)


				#endregion // Debug-Only Verification
			}

			#endregion // AddPaddingCharacters


			// MD 2/14/12 - 12.1 - Table Support
			#region AddScientificParts

			private void AddScientificParts(int numberOfScientificFractionalDigits)
			{
				this.AddWellKnownPart(WellKnownPartType.DigitOrZero);

				if (numberOfScientificFractionalDigits != 0)
				{
					this.AddWellKnownPart(WellKnownPartType.DecimalSeparator);
					this.AddWellKnownPart(WellKnownPartType.DigitOrZero);
					for (int i = 1; i < numberOfScientificFractionalDigits; i++)
						this.AddWellKnownPart(WellKnownPartType.DigitOrEmpty);
				}

				this.AddWellKnownPart(WellKnownPartType.ScientificWithSign);
				this.AddWellKnownPart(WellKnownPartType.DigitOrZero);
				this.AddWellKnownPart(WellKnownPartType.DigitOrZero);
			}

			#endregion // AddScientificParts

			#region AddWellKnownPart

			public void AddWellKnownPart(WellKnownPartType type)
			{
				this.AddWellKnownPart(type, 1);
			}

			public void AddWellKnownPart(WellKnownPartType type, int repeatCount)
			{
				if (WellKnownPart.IsDigit(type))
				{
					this.cachedGeneratedDenominatorDigitCount = -1;
					this.cachedGeneratedNumeratorDigitCount = -1;
					this.cachedNumberDigitCount = -1;
					this.cachedFractionDigitCount = -1;
					this.cachedScientificExponentDigitCount = -1;
				}

				if (WellKnownPart.IsDateTime(type))
					this.SetSectionType(SectionType.Date);

				switch (type)
				{
					case WellKnownPartType.AMPMLong:
					case WellKnownPartType.AMPMShortLL:
					case WellKnownPartType.AMPMShortLU:
					case WellKnownPartType.AMPMShortUL:
					case WellKnownPartType.AMPMShortUU:
					// MD 3/24/12 - TFS105636
					// Added support for the Chinese AM/PM format code.
					case WellKnownPartType.AMPMChinese:
						this.hasAMPMPart = true;
						break;

					case WellKnownPartType.DecimalSeparator:
						this.hasDecimalSeparator = true;
						break;

					case WellKnownPartType.DigitOrEmpty:
					case WellKnownPartType.DigitOrZero:
					case WellKnownPartType.DigitOrWhitespace:
						{
							this.SetSectionType(SectionType.Number);

							WellKnownPart wellKnownPart = this.LastPart as WellKnownPart;

							// If the digit character is preceeded by another digit or a decimal separator, keep the group shift count the way it is. 
							// For all other characters, reset it.
							if (wellKnownPart == null ||
								(WellKnownPart.IsDigit(wellKnownPart.Type) == false && wellKnownPart.Type != WellKnownPartType.DecimalSeparator))
							{
								this.ResetGroupShiftCount();
							}
						}
						break;

					case WellKnownPartType.FractionSeparator:
						{
							if (this.hasFractionSeparator || this.hasScientificNotation)
							{
								this.isValid = false;
								return;
							}

							this.hasFractionSeparator = true;

							bool foundNonNumericPart = false;

							// MD 3/16/12 - TFS105094
							this.generatedFractionIsOptional = true;

							for (int i = this.parts.Count - 1; i >= 0; i--)
							{
								Part part = this.parts[i];
								WellKnownPart wellKnownPart = part as WellKnownPart;

								if (wellKnownPart == null)
								{
									foundNonNumericPart = true;
									continue;
								}

								if (this.generatedNumeratorPartIndex.HasValue)
								{
									if (wellKnownPart.Type == WellKnownPartType.DecimalSeparator)
									{
										this.isValid = false;
										return;
									}

									if (WellKnownPart.IsDigit(wellKnownPart.Type))
									{
										if (foundNonNumericPart)
											this.generatedFractionUsesFullNumber = false;
										else
											this.generatedNumeratorPartIndex = i;
									}
								}
								else
								{
									if (wellKnownPart.Type == WellKnownPartType.DecimalSeparator ||
										WellKnownPart.IsDigit(wellKnownPart.Type))
									{
										this.generatedNumeratorPartIndex = i;
									}
								}

								// MD 3/16/12 - TFS105094
								// If we find a '0' format character in the numerator section, the generated fraction is no longer optional.
								if (foundNonNumericPart == false &&
									wellKnownPart.Type == WellKnownPartType.DigitOrZero)
								{
									this.generatedFractionIsOptional = false;
								}
							}
						}
						break;

					case WellKnownPartType.GeneralString:
						this.SetSectionType(SectionType.Text);
						break;

					case WellKnownPartType.GroupShiftPlaceholder:
						{
							// Walk backwards over the parts. If there are any literal parts between this and any previous GroupShiftPlaceholders, 
							// the group shift gets reset and only the last one is used.
							for (int i = this.parts.Count - 1; i >= 0; i--)
							{
								Part part = this.parts[i];
								WellKnownPart wellKnownPart = part as WellKnownPart;

								if (wellKnownPart == null)
								{
									this.ResetGroupShiftCount();
									break;
								}

								if (wellKnownPart.Type == WellKnownPartType.GroupShiftPlaceholder)
									break;
							}

							this.groupShiftCount += repeatCount;
						}
						break;

					// MD 2/8/12 - 12.1 - Table Support
					case WellKnownPartType.Milliseconds1:
					case WellKnownPartType.Milliseconds2:
					case WellKnownPartType.Milliseconds3:
						this.hasMillisecondPart = true;
						break;

					case WellKnownPartType.Percent:
						this.SetSectionType(SectionType.Number);
						this.percentCount++;
						break;

					case WellKnownPartType.Scientific:
					case WellKnownPartType.ScientificWithSign:
						this.SetSectionType(SectionType.Number);

						if (this.hasScientificNotation || this.hasFractionSeparator)
						{
							this.isValid = false;
							return;
						}

						this.hasScientificNotation = true;
						this.ResetGroupShiftCount();
						break;

					case WellKnownPartType.VerbatimString:
						this.hasVerbatimString = true;
						this.SetSectionType(SectionType.Text);
						break;

					// MD 3/19/12 - TFS105157
					// Anytime after a 'g' format code, the 'y' format codes are treated like 'e' format codes.
					case WellKnownPartType.YearsShort:
					case WellKnownPartType.YearsLong:
						for (int i = this.parts.Count - 1; i >= 0; i--)
						{
							Part part = this.parts[i];
							WellKnownPart wellKnownPart = part as WellKnownPart;

							if (wellKnownPart == null)
								continue;

							switch (wellKnownPart.Type)
							{
								case WellKnownPartType.EraEnglishName:
								case WellKnownPartType.EraAbbreviatedName:
								case WellKnownPartType.EraName:
									type = WellKnownPartType.EraYearsLong;
									break;

								default:
									continue;
							}
							break;
						}
						break;

					// MD 3/19/12 - TFS105157
					// When the locale is already specified, the 'b' format codes are treated like 'y' format codes.
					case WellKnownPartType.BuddhistYearsLong:
						if (this.cultureForDates != null)
							type = WellKnownPartType.YearsLong;

						break;

					// MD 3/19/12 - TFS105157
					// When the locale is already specified, the 'b' format codes are treated like 'y' format codes.
					case WellKnownPartType.BuddhistYearsShort:
						if (this.cultureForDates != null)
							type = WellKnownPartType.YearsShort;

						break;
				}

				this.parts.Add(new WellKnownPart(type, repeatCount));
			}

			#endregion // AddWellKnownPart

			// MD 3/19/12 - TFS105157
			#region ConvertNumberTextToNativeDigits

			public string ConvertNumberTextToNativeDigits(string valueText)
			{



				if (this.shouldConvertNativeDigits == false)
					return valueText;

				NumberFormatInfo numberFormatInfo = this.CultureForDatesResolved.NumberFormat;
				string[] nativeDigits = numberFormatInfo.NativeDigits;

				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < valueText.Length; i++)
				{
					char currentChar = valueText[i];
					if ('0' <= currentChar && currentChar <= '9')
						sb.Append(nativeDigits[currentChar - '0']);
					else
						sb.Append(currentChar);
				}
				return sb.ToString();

			}

			#endregion // ConvertNumberTextToNativeDigits

			// MD 3/19/12 - TFS105157
			#region CultureFormatDay

			public string CultureFormatDay(DateTime dateTime, int requiredNumberOfDigits)
			{
				int day;
				if (dateTime.Year == 1899 && this.CalendarResolved.GetYear(dateTime) == 1899)
					day = 0;
				else
					day = this.CalendarResolved.GetDayOfMonth(dateTime);

				return this.CultureFormatNumber(day, requiredNumberOfDigits);
			}

			#endregion // CultureFormatDay

			// MD 3/19/12 - TFS105157
			#region CultureFormatMonth

			public string CultureFormatMonth(DateTime dateTime, int requiredNumberOfDigits)
			{
				int month;
				if (dateTime.Year == 1899 && this.CalendarResolved.GetYear(dateTime) == 1899)
					month = 1;
				else
					month = this.CalendarResolved.GetMonth(dateTime);

				return this.CultureFormatNumber(month, requiredNumberOfDigits);
			}

			#endregion // CultureFormatMonth

			// MD 3/19/12 - TFS105157
			#region CultureFormatNumber

			public string CultureFormatNumber(double value)
			{
				string valueText = value.ToString(this.Culture);

				// If the '@' format code is present, the native digits are not used, so return the text as is.
				if (this.HasVerbatimString)
					return valueText;

				return ConvertNumberTextToNativeDigits(valueText);
			}

			public string CultureFormatNumber(int value)
			{
				return this.CultureFormatNumber(value, 1);
			}

			private static readonly string[] InvariantCultureNativeDigits = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

			public string CultureFormatNumber(int value, int requiredNumberOfDigits)
			{
				NumberFormatInfo numberFormatInfo = this.CultureForDatesResolved.NumberFormat;

				string[] nativeDigits = FormatSection.InvariantCultureNativeDigits;


				// The native digits are only used when the '@' format code is not present.
				if (this.HasVerbatimString == false)
					nativeDigits = numberFormatInfo.NativeDigits;


				StringBuilder sb = new StringBuilder(requiredNumberOfDigits);
				while (true)
				{
					int currentDigitValue;
					if (value == 0)
					{
						if (sb.Length >= requiredNumberOfDigits)
							break;

						currentDigitValue = 0;
					}
					else
					{
						currentDigitValue = value % 10;
						value /= 10;
					}

					sb.Insert(0, nativeDigits[currentDigitValue]);
				}

				return sb.ToString();
			}

			#endregion // CultureFormatNumber

			// MD 3/19/12 - TFS105157
			#region CultureFormatYear

			public string CultureFormatYear(DateTime dateTime, int requiredNumberOfDigits)
			{
				// MD 3/26/12 - 12.1 - Table Support
				// Moved code to new overload which takes the calendar.
				return this.CultureFormatYear(this.CalendarResolved, dateTime, requiredNumberOfDigits);
			}

			// MD 3/26/12 - 12.1 - Table Support
			public string CultureFormatYear(Calendar calendar, DateTime dateTime, int requiredNumberOfDigits)
			{
				int year = calendar.GetYear(dateTime);
				if (dateTime.Year == 1899 && year == 1899)
					year = 1900;

				if (requiredNumberOfDigits == 2)
					year %= 100;

				return this.CultureFormatNumber(year, requiredNumberOfDigits);
			}

			#endregion // CultureFormatYear

			#region FormatText

			// MD 2/14/12 - 12.1 - Table Support
			// Added parameters for the information needed to measure and constrain the text.
			//public string FormatText(string text)
			public string FormatText(string text

				, object cellValue, GetTextWidthHelper getTextWidthHelper, int availablePixels, out bool useHashes

				)
			{

				// MD 2/14/12 - 12.1 - Table Support
				useHashes = false;


				Debug.Assert(this.sectionType == SectionType.Text || this.sectionType == SectionType.Default, "The format type should be Text.");

				// MD 2/14/12 - 12.1 - Table Support
				int paddingPosition = -1;
				char paddingChar = (char)0;

				StringBuilder formattedText = new StringBuilder(text.Length);

				// MD 3/23/12 - TFS106028
				// If this section is the 1st, 2nd, or 3rd section and there is no verbatim string format characters,
				// we should just return the text as is.
				if (this.HasVerbatimString == false && this.sectionIndex != ValueFormatter.TextSection)
				{
					formattedText.Append(text);
				}
				else
				{
					foreach (Part part in this.parts)
					{
						// MD 2/14/12 - 12.1 - Table Support
						// If the 'General' string is anywhere in the format section and the value is text, the text will be written out as is, 
						// without including literals.
						WellKnownPart wellKnownPart = part as WellKnownPart;
						if (wellKnownPart != null && wellKnownPart.Type == WellKnownPartType.GeneralString)
						{
							formattedText = new StringBuilder(text);
							break;
						}

						string partText = part.GetText(this, text);

						// MD 2/14/12 - 12.1 - Table Support
						// Keep track of the last padding character, because it will be repeated if we are honoring the cell width.
						LiteralFormatPart literalPart = part as LiteralFormatPart;
						if (literalPart != null && literalPart.IsRepeated)
						{
							Debug.Assert(partText != null && partText.Length == 0, "The repeat text should not have been included.");
							Debug.Assert(literalPart.Text.Length == 1, "The repeat text should only have one character.");
							paddingPosition = formattedText.Length;
							paddingChar = literalPart.Text[0];
						}

						if (partText == null)
							return null;

						formattedText.Append(partText);
					}
				}

				// MD 2/14/12 - 12.1 - Table Support
				// If we need to honor the cell width, measure the text so we can add in padding characters or display all hashes.
				//return formattedText.ToString();
				string result = formattedText.ToString();

				if (getTextWidthHelper != null)
				{
					int width = getTextWidthHelper.GetWidth(this, result);
					if (width <= availablePixels)
					{
						this.AddPaddingCharacters(getTextWidthHelper, availablePixels, paddingPosition, paddingChar, ref result, ref width);
					}
					else if (cellValue is bool || paddingPosition >= 0)
					{
						// Boolean values are treated like text unless they can't fit in the cell, in which case they should display all hashes like numbers,
						// so return null here. This is also true for string when padding is included.
						useHashes = true;
						return null;
					}
				}


				return result;
			}

			#endregion // FormatText

			#region FormatValue

			// MD 2/14/12 - 12.1 - Table Support
			// Added parameters for the information needed to measure and constrain the text.
			//public string FormatValue(double value)
			public string FormatValue(double value

					, GetTextWidthHelper getTextWidthHelper, int availablePixels, out bool useRoundedValue, out SectionType formattedAs

					)
			{

				// MD 2/14/12 - 12.1 - Table Support
				// Moved all code to a new overload.
				int usedFractionDigits;
				return this.FormatValue(value, getTextWidthHelper, availablePixels, out useRoundedValue, out usedFractionDigits, out formattedAs);
			}

			// MD 2/14/12 - 12.1 - Table Support
			// Added a new overload so recursive calls can determine how many fraction digits were actually used when creating the value.
			public string FormatValue(double value, GetTextWidthHelper getTextWidthHelper, int availablePixels, out bool useRoundedValue, out int usedFractionDigits, out SectionType formattedAs)
			{

				// MD 2/14/12 - 12.1 - Table Support
				useRoundedValue = false;
				usedFractionDigits = 0;
				formattedAs = this.SectionType;

				// MD 2/14/12 - 12.1 - Table Support
				// In numeric sections which are regular numbers with no decimal portion, remove the decimal portion so we don't try to display 
				// it as a scientific value.
				if (this.SectionType == ValueFormatter.SectionType.Number &&
					this.HasScientificNotation == false &&
					this.HasFractionSeparator == false &&
					this.HasDecimalSeparator == false &&
					this.PercentCount == 0)
				{
					value = MathUtilities.MidpointRoundingAwayFromZero(value);
				}

				StringBuilder formattedText = new StringBuilder();

				if (value < 0)
				{
					bool includeSign = false;
					switch (this.sectionIndex)
					{
						case ValueFormatter.PositiveNumberSection:
							includeSign = (this.hasCustomComparison == false || this.compareOperand != 0);
							break;

						case ValueFormatter.NegativeNumberSection:
							
							if (this.formatter.formatSections[ValueFormatter.ZeroNumberSection] == null)
							{
								if (this.HasVerbatimString)
								{
									includeSign = true;
								}
								else if (this.HasCustomComparison == false)
								{
									FormatSection firstSection = this.formatter.formatSections[ValueFormatter.PositiveNumberSection];
									if (firstSection.HasCustomComparison && firstSection.compareOperand != 0)
									{
										includeSign = true;
									}
								}
								
							}
							break;

						case ValueFormatter.ZeroNumberSection:
							includeSign = true;
							break;

						default:
							Utilities.DebugFail("Unknown section.");
							break;
					}

					if (includeSign)
						formattedText.Append(this.culture.NumberFormat.NegativeSign);
				}

				FormatValueState state = new FormatValueState(this, value);

				// MD 2/14/12 - 12.1 - Table Support
				#region Old Code

				//foreach (Part part in this.parts)
				//{
				//    string partText = part.GetText(this, state);

				//    if (partText == null)
				//        return null;

				//    formattedText.Append(partText);
				//}

				//return formattedText.ToString();

				#endregion // Old Code
				int originalFormattedTextLength = formattedText.Length;
				int minimumFractionDigitsNeeded = Math.Max(0, -(int)Math.Floor(Math.Log10(Math.Abs(value))));
				int decimalSizeCorrection = -1;

				bool hasGeneralPart = false;
				bool hasVerbatimPart = false;
				bool triedUsingScientificValues = false;
				while (true)
				{
					int paddingPosition = -1;
					char paddingChar = (char)0;

					if (this.SectionType == ValueFormatter.SectionType.Text && this.HasVerbatimString)
					{
						// When the verbatim string ('@') is present, we don't use any literals.
						hasVerbatimPart = true;
						formattedText.Append(this.GetVerbatimString(state));
					}
					else
					{
						// MD 3/25/12 - TFS104630
						// Certain locale IDs indicate that we should use the date/time patterns in the current format, so don't use 
						// the parts collection.
						if (this.forcedLongDateStyle)
						{
							if (state.DateTime.HasValue == false)
								return null;

							formattedText.Append(state.DateTime.Value.ToLongDateString());
						}
						else if (this.forcedLongTimeStyle)
						{
							if (state.DateTime.HasValue == false)
								return null;

							formattedText.Append(state.DateTime.Value.ToLongTimeString());
						}
						else
						{
							foreach (Part part in this.parts)
							{
								string partText = part.GetText(this, state);

								// Keep track of the last padding character, because it will be repeated if we are honoring the cell width.
								LiteralFormatPart literalPart = part as LiteralFormatPart;
								if (literalPart != null && literalPart.IsRepeated)
								{
									Debug.Assert(partText != null && partText.Length == 0, "The repeat text should not have been included.");
									Debug.Assert(literalPart.Text.Length == 1, "The repeat text should only have one character.");
									paddingPosition = formattedText.Length;
									paddingChar = literalPart.Text[0];
								}
								else
								{
									// Keep track of whether there is at least one general string.
									WellKnownPart wellKnownPart = part as WellKnownPart;
									if (wellKnownPart != null && wellKnownPart.Type == WellKnownPartType.GeneralString)
										hasGeneralPart = true;
								}

								if (partText == null)
									return null;

								formattedText.Append(partText);
							}
						}
					}


					usedFractionDigits = state.FractionFormatDigitsUsed; 


					string result = formattedText.ToString();


					// If there is no DC to measure the text, just return it. We are not honoring the cell width.
					if (getTextWidthHelper == null)
						return result;

					int width = getTextWidthHelper.GetWidth(this, result);
					int availablePixelsCorrected = availablePixels;

					// These seem like bugs in MS Excel, but there are some quirks in their measuring logic which we will try to mimic.
					for (int i = 0; i < result.Length; i++)
					{
						char currentChar = result[i];
						if (currentChar == '.')
						{
							// MD 3/24/12 - TFS105618
							// The decimal point size correction only seems to be used for non-percent format strings.
							if (this.PercentCount != 0)
								continue;

							if (decimalSizeCorrection < 0)
								decimalSizeCorrection = Math.Max(0, getTextWidthHelper.GetWidth(this, "0") - getTextWidthHelper.GetWidth(this, "."));

							availablePixelsCorrected -= decimalSizeCorrection;
						}
						else if (this.HasScientificNotation && currentChar == 'E' && i < result.Length - 1)
						{
							if ((result[i + 1].ToString() == this.culture.NumberFormat.NegativeSign || result[i + 1].ToString() == this.culture.NumberFormat.PositiveSign))
							{
								availablePixelsCorrected -= Math.Max(0, getTextWidthHelper.GetWidth(this, "0") - getTextWidthHelper.GetWidth(this, result[i + 1].ToString()));
							}
						}
						else
						{
							// Excel seems to measure an extra pixel per Asian character, so remove one from the available width.
							UnicodeCategory category = Char.GetUnicodeCategory(currentChar);
							if (category == UnicodeCategory.OtherLetter)
								availablePixelsCorrected--;
						}
					}

					// If the text fits, add any padding characters if necessary and return the text.
					if (width <= availablePixelsCorrected)
					{
						// Padding characters are added with the original available space, not the corrected available space to match MS Excel's behavior 
						// when showing the value as a scientific value with a decimal portion to save space.
						this.AddPaddingCharacters(getTextWidthHelper, availablePixels, paddingPosition, paddingChar, ref result, ref width);
						return result;
					}

					// If the value is zero and it could not fit, we can't shrink it, so return null so that we'll use all hashes.
					if (value == 0)
						return null;

					// If we have a general or verbatim string part, we can try to shrink the value. Otherwise, return null so that we'll use all hashes.
					if (hasGeneralPart == false && hasVerbatimPart == false)
						return null;

					// When there is a verbatim string part ('@'), we can only try to shrink the value when it is the first section.
					// Otherwise, the value won't fit so return null so that we'll use all hashes.
					if (hasVerbatimPart && this.sectionIndex != 0)
						return null;

					// First try to remove digits from the number if there is a fraction portion.
					if (0 < state.FractionDigitsUsedInVerbatimString && minimumFractionDigitsNeeded < state.FractionDigitsUsedInVerbatimString)
					{
						state.FractionDigitsAllowedInVerbatimString = state.FractionDigitsUsedInVerbatimString - 1;
					}
					else
					{
						if (triedUsingScientificValues)
							return null;

						// If we can't remove any more fraction digits, try to make it a scientific value. 
						// Set the flag to indicate we have done this so we don't try this twice.
						triedUsingScientificValues = true;

						for (int numberOfScientificFractionalDigits = 5;
							numberOfScientificFractionalDigits >= 0;
							numberOfScientificFractionalDigits--)
						{
							// When the verbatim string ('@') is present, we don't use any literals.
							// Replace the general or verbatim string with the scientific parts.
							// MD 4/9/12 - TFS101506
							//FormatSection section = new FormatSection(this.formatter, this.sectionIndex);
							FormatSection section = new FormatSection(this.formatter, this.sectionIndex, this.culture);

							if (hasVerbatimPart)
							{
								section.AddScientificParts(numberOfScientificFractionalDigits);
							}
							else
							{
								foreach (Part part in this.parts)
								{
									WellKnownPart wellKnownPart = part as WellKnownPart;
									if (wellKnownPart != null)
									{
										if (wellKnownPart.Type == WellKnownPartType.GeneralString)
											section.AddScientificParts(numberOfScientificFractionalDigits);
										else
											section.AddWellKnownPart(wellKnownPart.Type);
									}
									else
									{
										LiteralFormatPart literalPart = part as LiteralFormatPart;
										if (literalPart != null)
										{
											if (literalPart.IsRepeated)
												section.AddLiteralFormatPart(literalPart.Text[0], literalPart.IsRepeated);
											else
												section.AddLiteralFormatPart(literalPart.Text);
										}
										else
										{
											Utilities.DebugFail("Unknown part type.");
										}
									}
								}
							}

							// Try to format the value. If it formats correctly, fits in the cell and used all digits we allocated with the scientific parts,
							// return the text. Otherwise, remove one fraction digit from the coefficient and try again.
							int usedFractionDigitsForScientificValue;
							string scientificResult = section.FormatValue(value, getTextWidthHelper, availablePixels, out useRoundedValue, out usedFractionDigitsForScientificValue, out formattedAs);
							if (scientificResult != null && usedFractionDigitsForScientificValue == numberOfScientificFractionalDigits)
								return scientificResult;
						}

						// If there were no fraction digits and using a scientific value didn't fit, return null so we'll use all hashes.
						if (minimumFractionDigitsNeeded == 0)
							return null;

						// If this is a verbatim string or the general string has literals with it, we should round off the fractional part and try again.
						if (hasVerbatimPart || this.parts.Count > 1)
						{
							useRoundedValue = true;
							return null;
						}

						// Otherwise, if this is a general string with no literal, allow no fractional digits and try one last time to fit the value.
						// (This seems like we should just set useRoundedValue to True and return null, like we do above, here, but there is a slight
						// difference: when a negative value such as -0.01 is used and we can't fit the fraction digits, the "General" format string 
						// will display it as "-0" whereas the "@" format string will display it as "0".
						state.FractionDigitsAllowedInVerbatimString = 0;
					}

					// Reset the string builder to where it was before we adding in any part strings so we can try again with a shrunken value.
					formattedText.Length = originalFormattedTextLength;



				}
			}

			#endregion // FormatValue

			#region GeneratedFractionUsesFullNumber

			public bool GeneratedFractionUsesFullNumber
			{
				get { return this.generatedFractionUsesFullNumber; }
			}

			#endregion // GeneratedFractionUsesFullNumber

			#region GetLastDateTimePart

			public WellKnownPart GetLastDateTimePart()
			{
				for (int i = this.parts.Count - 1; i >= 0; i--)
				{
					WellKnownPart wellKnownPart = this.parts[i] as WellKnownPart;

					if (wellKnownPart != null && WellKnownPart.IsDateTime(wellKnownPart.Type))
						return wellKnownPart;
				}

				return null;
			}

			#endregion // GetLastDateTimePart

			// MD 2/8/12 - 12.1 - Table Support
			#region GetVerbatimString

			public string GetVerbatimString(FormatValueState state)
			{
				double value = state.AbsoluteValue;

				// MD 2/14/12 - 12.1 - Table Support
				// We want to use the FractionDigitsAllowedInVerbatimString restriction below, so don't do this shortcut anymore.
				// However, if the value is 0, it will not have any fractions digits and the code below will throw an error, so just
				// return the ToString of the number in this case.
				//string text = value.ToString();
				//
				//if (text.Length <= 11)
				//    return text;
				if (value == 0)
				{
					// MD 3/24/12
					// Found while fixing TFS105618
					// We need to use the native digits here.
					//return value.ToString();
					return this.CultureFormatNumber(value);
				}

				// MD 3/24/12 - TFS105618
				// We can only auto-format into a scientific value if we are not trying to restrict the number of digits after the 
				// decimal to fit the value.
				//if (value < 0.0001 || 99999999999 < value)
				if (state.FractionDigitsAllowedInVerbatimString == -1 &&
					(value < 0.0001 || 99999999999 < value))
				{
					int exponent = (int)Math.Floor(Math.Log10(value));

					// MD 3/24/12
					// Found while fixing TFS105618
					// We need to use the native digits here.
					//string exponentText = Math.Abs(exponent).ToString("00");
					string exponentText = this.CultureFormatNumber(Math.Abs(exponent), 2);

					int roundingDigits = Math.Max(0, 7 - exponentText.Length);
					double coefficient = value / Math.Pow(10, exponent);

					// MD 3/24/12
					// Found while fixing TFS105618
					// We need to use the native digits here.
					//string coefficientText = Math.Round(coefficient, roundingDigits).ToString();
					string coefficientText = this.CultureFormatNumber(Math.Round(coefficient, roundingDigits));

					string exponentSign = exponent < 0 ? this.Culture.NumberFormat.NegativeSign : this.Culture.NumberFormat.PositiveSign;
					return coefficientText + "E" + exponentSign + exponentText;
				}
				else
				{
					// MD 2/14/12 - 12.1 - Table Support
					// We may have to restriction the number of fraction digits are used in the returned value.
					// Also, we need to keep track of how many were used.
					//if (value < 1)
					//    return ValueFormatter.Round(value, 9).ToString();
					//
					//int significantDigitsBeforeDecimal = (int)Math.Floor(Math.Log10(value)) + 1;
					//int roundingDigits = Math.Max(0, 10 - significantDigitsBeforeDecimal);
					//return ValueFormatter.Round(value, roundingDigits).ToString();

					int fractionDigits;
					if (value < 1)
					{
						fractionDigits = 9;
					}
					else
					{
						int significantDigitsBeforeDecimal = (int)Math.Floor(Math.Log10(value)) + 1;
						fractionDigits = Math.Max(0, 10 - significantDigitsBeforeDecimal);
					}

					if (state.FractionDigitsAllowedInVerbatimString != -1)
						fractionDigits = Math.Min(state.FractionDigitsAllowedInVerbatimString, fractionDigits);

					state.FractionDigitsUsedInVerbatimString = fractionDigits;

					// MD 3/24/12
					// Found while fixing TFS105618
					// We need to use the native digits here.
					//return ValueFormatter.Round(value, fractionDigits).ToString();
					return this.CultureFormatNumber(ValueFormatter.Round(value, fractionDigits));
				}
			}

			#endregion // GetVerbatimString

			#region HasFractionSeparator

			public bool HasFractionSeparator
			{
				get { return this.hasFractionSeparator; }
			}

			#endregion // HasFractionSeparator

			// MD 2/8/12 - 12.1 - Table Support
			#region HasMillisecondPart

			public bool HasMillisecondPart
			{
				get { return this.hasMillisecondPart; }
			}

			#endregion // HasMillisecondPart

			#region HasVerbatimString

			public bool HasVerbatimString
			{
				get { return this.hasVerbatimString; }
			}

			#endregion // HasVerbatimString

			#region InsertGroupSeparators

			public void InsertGroupSeparators(ref string text, int currentDigitPosition, string numberText)
			{
				this.InsertGroupSeparators(ref text, currentDigitPosition, numberText, this.culture.NumberFormat.NumberGroupSeparator);
			}

			public void InsertGroupSeparators(ref string text, int currentDigitPosition, string numberText, string separator)
			{
				if (this.shouldShowGroupSeparators == false)
					return;

				int[] groupSizes = this.culture.NumberFormat.NumberGroupSizes;
				int originalTextLength = text.Length;
				int distanceFromsOnesUnit = (this.NumberDigitCount - currentDigitPosition) + (originalTextLength - 1) - 1;

				int separatorNeededAt = 0;
				for (int i = 0; i < groupSizes.Length; i++)
				{
					int groupSize = groupSizes[i];
					separatorNeededAt += groupSize;

					if (distanceFromsOnesUnit < separatorNeededAt)
						break;

					if (distanceFromsOnesUnit - (originalTextLength - 1) <= separatorNeededAt)
					{
						text = text.Insert(
							(distanceFromsOnesUnit - separatorNeededAt) + 1,
							separator);
					}

					if (i == groupSizes.Length - 1)
					{
						if (groupSize == 0)
							break;

						i--;
					}
				}
			}

			#endregion // InsertGroupSeparators

			#region MeetsCriteria

			public bool MeetsCriteria(double value)
			{
				switch (this.compareOperator)
				{
					// MD 2/14/12 - 12.1 - Table Support
					case CompareOperator.AnyValue:
						return true;

					case CompareOperator.Equals:
						return value == this.compareOperand;

					case CompareOperator.NotEquals:
						return value != this.compareOperand;

					case CompareOperator.LessThan:
						return value < this.compareOperand;

					case CompareOperator.LessThanOrEquals:
						return value <= this.compareOperand;

					case CompareOperator.GreaterThan:
						return value > this.compareOperand;

					case CompareOperator.GreaterThanOrEquals:
						return value >= this.compareOperand;

					default:
						Utilities.DebugFail("Unknown CompareOperator: " + this.compareOperator);
						return true;
				}
			}

			#endregion // MeetsCriteria

			#region ResetGroupShiftCount

			public void ResetGroupShiftCount()
			{
				this.groupShiftCount = 0;
			}

			#endregion // ResetGroupShiftCount

			#region SetColor

			public bool SetColor(string color)
			{
				if (this.color != null)
					return false;

				this.color = color;
				return true;
			}

			#endregion // SetColor

			#region SetComparisonType

			public bool SetComparisonType(CompareOperator compareOperator, double compareOperand)
			{
				// MD 2/14/12 - 12.1 - Table Support
				// Moved code to a new overload.
				return this.SetComparisonType(compareOperator, compareOperand, true);
			}

			// MD 2/14/12 - 12.1 - Table Support
			// Added a new overload so the hasCustomComparison value could be specified.
			public bool SetComparisonType(CompareOperator compareOperator, double compareOperand, bool hasCustomComparison)
			{
				if (this.hasCustomComparison)
					return false;

				if (this.sectionIndex >= ValueFormatter.ZeroNumberSection)
					return false;

				// MD 2/14/12 - 12.1 - Table Support
				//this.hasCustomComparison = true;
				this.hasCustomComparison = hasCustomComparison;

				this.compareOperator = compareOperator;
				this.compareOperand = compareOperand;
				return true;
			}

			#endregion // SetComparisonType

			#region SetLocaleId

			public bool SetLocaleId(int localeId)
			{
				if (this.cultureForDates != null)
					return false;

				if (localeId == 0xF800)
				{
					// MD 3/25/12 - TFS104630
					// This was incorrect. The 0xF800 locale ID actually means we should use the current culture's long date pattern.
					#region Old Code

					//this.cultureForDates = CultureInfo.InvariantCulture;
					//this.forcedLongDateStyle = true;
					//this.SetSectionType(SectionType.Date);

					//this.parts.Clear();
					//this.AddWellKnownPart(WellKnownPartType.DayLongDescription);
					//this.AddLiteralFormatPart(", ");
					//this.AddWellKnownPart(WellKnownPartType.MonthLongDescription);
					//this.AddLiteralFormatPart(' ');
					//this.AddWellKnownPart(WellKnownPartType.DaysLong);
					//this.AddLiteralFormatPart(", ");
					//this.AddWellKnownPart(WellKnownPartType.YearsLong);

					#endregion // Old Code
					this.parts.Clear();
					this.forcedLongDateStyle = true;

					// MD 4/3/12 - TFS107459
					// When this locale id is specified, this is a date section.
					this.SetSectionType(ValueFormatter.SectionType.Date);

					return true;
				}
				else if (localeId == 0xF400)
				{
					// MD 3/25/12 - TFS104630
					// This was incorrect. The 0xF400 locale ID actually means we should use the current culture's long time pattern.
					#region Old Code

					//this.cultureForDates = CultureInfo.InvariantCulture;
					//this.forcedLongTimeStyle = true;
					//this.SetSectionType(SectionType.Date);

					//this.parts.Clear();
					//this.AddWellKnownPart(WellKnownPartType.HoursShort);
					//this.AddLiteralFormatPart(':');
					//this.AddWellKnownPart(WellKnownPartType.MinutesLong);
					//this.AddLiteralFormatPart(':');
					//this.AddWellKnownPart(WellKnownPartType.SecondsLong);
					//this.AddLiteralFormatPart(' ');
					//this.AddWellKnownPart(WellKnownPartType.AMPMLong);

					#endregion // Old Code
					this.parts.Clear();
					this.forcedLongTimeStyle = true;

					// MD 4/3/12 - TFS107459
					// When this locale id is specified, this is a date section.
					this.SetSectionType(ValueFormatter.SectionType.Date);

					return true;
				}

				try
				{
					// MD 3/19/12 - TFS105157
					// The values seems to contain more information that just the locale ID.
					int otherId = Utilities.GetBits(localeId, 24, 31);
					int calendarId = Utilities.GetBits(localeId, 16, 23);
					localeId = Utilities.GetBits(localeId, 0, 15);

					
					if (localeId == 0)
					{
						switch (otherId)
						{
							case 0x01:
							case 0x0D:
								localeId = 0x041E;
								break;
						}
					}


					this.cultureForDates = CultureInfo.GetCultureInfo(localeId);

					// MD 3/19/12 - TFS105157
					string[] nativeDigits = this.cultureForDates.NumberFormat.NativeDigits;
					for (int i = 0; i < 10; i++)
					{
						if (i.ToString() != nativeDigits[i])
						{
							this.shouldConvertNativeDigits = true;
							break;
						}
					}


#region Infragistics Source Cleanup (Region)

























































































































































































































#endregion // Infragistics Source Cleanup (Region)


					// MD 3/19/12 - TFS105157
					// If we are using a Japanese culture, the calendar for eras should use a Japanese calendar.
					if (this.cultureForDates.TwoLetterISOLanguageName == "ja")
						calendarForEras = new JapaneseCalendar();

					// MD 3/19/12 - TFS105157
					// Determine the calendar to use for non-era dates.
					switch (calendarId)
					{
						case 0:
							this.calendar = this.cultureForDates.Calendar;
							break;

						case 1:
						case 2:
						case 9:
						case 10:
						case 11:
						case 12:
							this.calendar = new GregorianCalendar((GregorianCalendarTypes)calendarId);
							break;

						case 3:
							this.calendar = new JapaneseCalendar();
							break;

						case 4:
							this.calendar = new TaiwanCalendar();
							break;

						case 5:
							this.calendar = new KoreanCalendar();
							break;

						case 6:
							this.calendar = new HijriCalendar();
							break;

						case 7:
							this.calendar = new ThaiBuddhistCalendar();
							break;

						case 8:
							this.calendar = new HebrewCalendar();
							break;


						case 13:
							this.calendar = new JulianCalendar();
							break;

						case 14:
							this.calendar = new JapaneseLunisolarCalendar();
							break;

						case 15:
							this.calendar = new ChineseLunisolarCalendar();
							break;

						case 20:
							this.calendar = new KoreanLunisolarCalendar();
							break;

						case 21:
							this.calendar = new TaiwanLunisolarCalendar();
							break;

						case 22:
							this.calendar = new PersianCalendar();
							break;


						case 23:
							this.calendar = new UmAlQuraCalendar();
							break;

						default:
							Utilities.DebugFail("Unknown calendar ID.");
							break;
					}
				}
				catch (Exception ex)
				{
					Utilities.DebugFail("Failed to set the Locale ID: " + ex.ToString());
				}

				return true;
			}

			#endregion // SetLocaleId

			#region SetSectionType

			public void SetSectionType(SectionType sectionType)
			{
				if (this.sectionType == sectionType)
					return;

				if (this.sectionType != SectionType.Default)
				{
					this.isValid = false;
					return;
				}

				this.sectionType = sectionType;
				return;
			}

			#endregion // SetSectionType

			#region ShowGroupSeparators

			public void ShowGroupSeparators()
			{
				this.shouldShowGroupSeparators = true;
			}

			#endregion // ShowGroupSeparators

			#endregion // Methods

			#region Properties

			// MD 3/19/12 - TFS105157
			#region CalendarResolved

			public Calendar CalendarResolved
			{
				get { return this.calendar ?? this.CultureForDatesResolved.Calendar; }
			}

			#endregion // CalendarResolved

			// MD 3/19/12 - TFS105157
			#region CalendarForErasResolved

			public Calendar CalendarForErasResolved
			{
				get { return this.calendarForEras ?? this.CalendarResolved; }
			}

			#endregion // CalendarForErasResolved

			#region Culture

			public CultureInfo Culture
			{
				get { return this.culture; }
			}

			#endregion // Culture

			#region CultureForDatesResolved

			public CultureInfo CultureForDatesResolved
			{
				get { return this.cultureForDates ?? this.culture; }
			}

			#endregion // CultureForDatesResolved

			#region Formatter

			public ValueFormatter Formatter
			{
				get { return this.formatter; }
			}

			#endregion // Formatter

			#region FractionDigitCount

			public int FractionDigitCount
			{
				get
				{
					if (this.hasDecimalSeparator == false)
						return 0;

					if (this.cachedFractionDigitCount < 0)
					{
						this.cachedFractionDigitCount = 0;
						bool foundDecimalSeparator = false;

						for (int i = 0; i < this.parts.Count; i++)
						{
							WellKnownPart wellKnownPart = this.parts[i] as WellKnownPart;
							if (wellKnownPart == null)
								continue;

							if (wellKnownPart.Type == WellKnownPartType.Scientific ||
								wellKnownPart.Type == WellKnownPartType.ScientificWithSign)
							{
								break;
							}

							if (wellKnownPart.Type == WellKnownPartType.DecimalSeparator)
								foundDecimalSeparator = true;
							else if (foundDecimalSeparator && WellKnownPart.IsDigit(wellKnownPart.Type))
								this.cachedFractionDigitCount++;
						}
					}

					return this.cachedFractionDigitCount;
				}
			}

			#endregion // FractionDigitCount

			#region GeneratedDenominatorSpecifiedValue

			public int? GeneratedDenominatorSpecifiedValue
			{
				get { return this.generatedDenominatorSpecifiedValue; }
				set { this.generatedDenominatorSpecifiedValue = value; }
			}

			#endregion // GeneratedDenominatorSpecifiedValue

			#region GeneratedDenominatorMaxDigitCount

			public int? GeneratedDenominatorMaxDigitCount
			{
				get { return this.generatedDenominatorMaxDigitCount; }
				set { this.generatedDenominatorMaxDigitCount = value; }
			}

			#endregion // GeneratedDenominatorMaxDigitCount

			#region GeneratedDenominatorDigitCount

			public int GeneratedDenominatorDigitCount
			{
				get
				{
					if (this.hasFractionSeparator == false)
						return 0;

					if (this.cachedGeneratedDenominatorDigitCount < 0)
					{
						this.cachedGeneratedDenominatorDigitCount = 0;
						bool foundFractionSeparator = false;

						for (int i = 0; i < this.parts.Count; i++)
						{
							WellKnownPart wellKnownPart = this.parts[i] as WellKnownPart;
							if (wellKnownPart == null)
								continue;

							if (wellKnownPart.Type == WellKnownPartType.FractionSeparator)
								foundFractionSeparator = true;
							else if (foundFractionSeparator && WellKnownPart.IsDigit(wellKnownPart.Type))
								this.cachedGeneratedDenominatorDigitCount++;
						}
					}

					return this.cachedGeneratedDenominatorDigitCount;
				}
			}

			#endregion // GeneratedDenominatorDigitCount

			// MD 3/16/12 - TFS105094
			#region GeneratedFractionIsOptional

			public bool GeneratedFractionIsOptional
			{
				get { return this.generatedFractionIsOptional; }
			}

			#endregion // GeneratedFractionIsOptional

			#region GeneratedNumeratorDigitCount

			public int GeneratedNumeratorDigitCount
			{
				get
				{
					if (this.hasFractionSeparator == false || this.generatedNumeratorPartIndex.HasValue == false)
						return 0;

					if (this.cachedGeneratedNumeratorDigitCount < 0)
					{
						this.cachedGeneratedNumeratorDigitCount = 0;

						for (int i = this.generatedNumeratorPartIndex.Value; i < this.parts.Count; i++)
						{
							WellKnownPart wellKnownPart = this.parts[i] as WellKnownPart;
							if (wellKnownPart == null)
								continue;

							if (wellKnownPart.Type == WellKnownPartType.FractionSeparator)
								break;

							if (WellKnownPart.IsDigit(wellKnownPart.Type))
								this.cachedGeneratedNumeratorDigitCount++;
						}
					}

					return this.cachedGeneratedNumeratorDigitCount;
				}
			}

			#endregion // GeneratedNumeratorDigitCount

			#region GroupShiftCount

			public int GroupShiftCount
			{
				get { return this.groupShiftCount; }
			}

			#endregion // GroupShiftCount

			#region HasAMPMPart

			public bool HasAMPMPart
			{
				get { return this.hasAMPMPart; }
			}

			#endregion // HasAMPMPart

			#region HasCustomComparison

			public bool HasCustomComparison
			{
				get { return this.hasCustomComparison; }
			}

			#endregion // HasCustomComparison

			#region HasDecimalSeparator

			public bool HasDecimalSeparator
			{
				get { return this.hasDecimalSeparator; }
			}

			#endregion // HasDecimalSeparator

			#region HasScientificNotation

			public bool HasScientificNotation
			{
				get { return this.hasScientificNotation; }
			}

			#endregion // HasScientificNotation

			#region IsForcedDateTimeStyle

			public bool IsForcedDateTimeStyle
			{
				get { return this.forcedLongDateStyle || this.forcedLongTimeStyle; }
			}

			#endregion // IsForcedDateTimeStyle

			#region IsValid

			public bool IsValid
			{
				get { return this.isValid; }
			}

			#endregion // IsValid

			#region LastPart

			public Part LastPart
			{
				get
				{
					if (this.parts.Count == 0)
						return null;

					return this.parts[this.parts.Count - 1];
				}
			}

			#endregion // LastPart

			#region NumberDigitCount






			public int NumberDigitCount
			{
				get
				{
					if (this.hasFractionSeparator && this.generatedFractionUsesFullNumber)
						return 0;

					if (this.cachedNumberDigitCount < 0)
					{
						this.cachedNumberDigitCount = 0;

						for (int i = 0; i < this.parts.Count; i++)
						{
							if (this.generatedNumeratorPartIndex.HasValue &&
								i == this.generatedNumeratorPartIndex.Value)
							{
								break;
							}

							WellKnownPart wellKnownPart = this.parts[i] as WellKnownPart;
							if (wellKnownPart == null)
								continue;

							if (wellKnownPart.Type == WellKnownPartType.DecimalSeparator ||
								wellKnownPart.Type == WellKnownPartType.Scientific ||
								wellKnownPart.Type == WellKnownPartType.ScientificWithSign)
								break;

							if (WellKnownPart.IsDigit(wellKnownPart.Type))
								this.cachedNumberDigitCount++;
						}
					}

					return this.cachedNumberDigitCount;
				}
			}

			#endregion // NumberDigitCount

			#region PercentCount

			public int PercentCount
			{
				get { return this.percentCount; }
			}

			#endregion // PercentCount

			#region ScientificExponentDigitCount

			public int ScientificExponentDigitCount
			{
				get
				{
					if (this.hasScientificNotation == false)
						return 0;

					if (this.cachedScientificExponentDigitCount < 0)
					{
						this.cachedScientificExponentDigitCount = 0;

						bool foundScientificSeparator = false;
						for (int i = 0; i < this.parts.Count; i++)
						{
							WellKnownPart wellKnownPart = this.parts[i] as WellKnownPart;
							if (wellKnownPart == null)
								continue;

							if (wellKnownPart.Type == WellKnownPartType.Scientific ||
								wellKnownPart.Type == WellKnownPartType.ScientificWithSign)
							{
								foundScientificSeparator = true;
								continue;
							}

							if (foundScientificSeparator == false)
								continue;

							if (wellKnownPart.Type == WellKnownPartType.DecimalSeparator)
								break;

							if (WellKnownPart.IsDigit(wellKnownPart.Type))
								this.cachedScientificExponentDigitCount++;
						}
					}

					return this.cachedScientificExponentDigitCount;
				}
			}

			#endregion // ScientificExponentDigitCount

			#region SectionType

			public SectionType SectionType
			{
				get { return this.sectionType; }
			}

			#endregion // SectionType

			#endregion // Properties
		}

		#endregion // FormatSection class

		#region FormatValueState class

		private class FormatValueState
		{
			#region Member Variables

			private double absoluteValue;
			private int currentDigitPosition;
			private NumberSection currentNumberSection;
			private DateTime? dateTime;

			// MD 2/14/12 - 12.1 - Table Support
			private int fractionDigitsAllowedInVerbatimString = -1;
			private int fractionDigitsUsedInVerbatimString = -1;
			private int fractionFormatDigitsUsed;

			private string fractionPortionText;
			private int generatedDenominator;
			private bool generatedDenominatorIsLeftAligned;
			private string generatedDenominatorText;
			private int generatedNumerator;
			private string generatedNumeratorText;
			private double numberPortion;
			private string numberPortionText;
			private double originalValue;
			private int scientificExponent;
			private string scientificExponentText;
			private FormatSection section;

			#endregion // Member Variables

			#region Constructor

			public FormatValueState(FormatSection section, double value)
			{
				this.section = section;
				this.originalValue = value;
				this.absoluteValue = Math.Abs(this.originalValue);

				if (section.HasFractionSeparator)
				{
					double valueForFraction;
					if (section.GeneratedFractionUsesFullNumber)
					{
						this.SetCurrentNumberSection(NumberSection.GeneratedNumerator);
						valueForFraction = this.absoluteValue;
					}
					else
					{
						this.numberPortion = ValueFormatter.Truncate(this.absoluteValue);

						// MD 3/19/12 - TFS105157
						// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
						//this.numberPortionText = this.numberPortion.ToString();
						this.numberPortionText = section.CultureFormatNumber(this.numberPortion);

						valueForFraction = this.absoluteValue - this.numberPortion;
					}

					if (section.GeneratedDenominatorSpecifiedValue.HasValue)
					{
						this.generatedDenominator = section.GeneratedDenominatorSpecifiedValue.Value;
						this.generatedNumerator = (int)ValueFormatter.Round(valueForFraction * this.generatedDenominator);
					}
					else if (section.GeneratedDenominatorMaxDigitCount.HasValue)
					{
						int numerator;
						int denominator;
						FormatValueState.Reduce(valueForFraction, section.GeneratedDenominatorMaxDigitCount.Value, out numerator, out denominator);

						this.generatedDenominator = denominator;
						this.generatedNumerator = numerator;
					}
					else
					{
						Utilities.DebugFail("Either the GeneratedFractionDenominator or the GeneratedFractionDigits should have a value.");
						return;
					}

					// MD 3/22/12 - TFS105610
					// If we have a number followed by a fraction and the fraction portion is equivalent to 1,
					// increment the number portion and make the fraction portion equal 0.
					if (this.generatedDenominator == this.generatedNumerator &&
						section.GeneratedFractionUsesFullNumber == false)
					{
						this.numberPortion += 1;
						this.numberPortionText = section.CultureFormatNumber(this.numberPortion);
						this.generatedNumerator = 0;
					}

					// MD 3/19/12 - TFS105157
					// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
					//this.generatedDenominatorText = this.generatedDenominator.ToString();
					//this.generatedNumeratorText = this.generatedNumerator.ToString();
					this.generatedDenominatorText = section.CultureFormatNumber(this.generatedDenominator);
					this.generatedNumeratorText = section.CultureFormatNumber(this.generatedNumerator);
				}
				else if (section.HasScientificNotation)
				{
					// MD 3/22/12 - TFS105608 
					// If the value is 0, we can't take the Log10 of it, so use 0 as the exponent.
					if (this.absoluteValue == 0)
					{
						this.scientificExponent = 0;
					}
					else
					{
						this.scientificExponent = (int)Math.Floor(Math.Log10(this.absoluteValue));

						int offset = this.scientificExponent % section.NumberDigitCount;
						if (this.scientificExponent % section.NumberDigitCount != 0)
						{
							if (this.scientificExponent < 0)
								this.scientificExponent -= section.NumberDigitCount + offset;
							else
								this.scientificExponent -= offset;
						}
					}

					// MD 3/19/12 - TFS105157
					// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
					//this.scientificExponentText = Math.Abs(this.scientificExponent).ToString();
					this.scientificExponentText = section.CultureFormatNumber(Math.Abs(this.scientificExponent));

					double coefficient = this.absoluteValue / Math.Pow(10, this.scientificExponent);
					coefficient = ValueFormatter.Round(coefficient, Math.Min(section.FractionDigitCount, 15));

					this.numberPortion = (int)ValueFormatter.Truncate(coefficient);

					// MD 3/19/12 - TFS105157
					// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
					//this.numberPortionText = this.numberPortion.ToString();
					this.numberPortionText = section.CultureFormatNumber(this.numberPortion);

					string coefficientText = coefficient.ToString();
					int decimalSeparatorIndex = coefficientText.IndexOf(this.section.Culture.NumberFormat.NumberDecimalSeparator);
					if (decimalSeparatorIndex < 0)
						this.fractionPortionText = string.Empty;
					else
						this.fractionPortionText = coefficientText.Substring(decimalSeparatorIndex + 1);

					this.SetCurrentNumberSection(NumberSection.ScientificCoefficient);
				}
				else
				{
					double valueResolved = this.absoluteValue;

					
					if (section.GroupShiftCount != 0)
						valueResolved /= Math.Pow(1000, section.GroupShiftCount);

					if (section.PercentCount != 0)
						valueResolved *= Math.Pow(100, section.PercentCount);

					if (valueResolved != 0 && section.SectionType != SectionType.Date)
						valueResolved = ValueFormatter.Round(valueResolved, Math.Min(section.FractionDigitCount, 15));

					this.numberPortion = ValueFormatter.Truncate(valueResolved);

					// MD 3/19/12 - TFS105157
					// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
					//this.numberPortionText = this.numberPortion.ToString();
					this.numberPortionText = section.CultureFormatNumber(this.numberPortion);

					string valueText = valueResolved.ToString();
					int decimalSeparatorIndex = valueText.IndexOf(this.section.Culture.NumberFormat.NumberDecimalSeparator);

					if (decimalSeparatorIndex < 0)
						this.fractionPortionText = string.Empty;
					else
						this.fractionPortionText = valueText.Substring(decimalSeparatorIndex + 1);
				}

				// MD 3/19/12 - TFS105157
				// The .NET number formatting logic does not use the native digits, so we may need to convert the digits here.
				if (String.IsNullOrEmpty(this.fractionPortionText) == false)
					this.fractionPortionText = section.ConvertNumberTextToNativeDigits(this.fractionPortionText);
			}

			#endregion // Constructor

			#region Methods

			#region OnDecimalSeparatorEncountered

			public void OnDecimalSeparatorEncountered()
			{
				switch (this.currentNumberSection)
				{
					case NumberSection.Number:
						this.SetCurrentNumberSection(NumberSection.Fraction);
						break;

					case NumberSection.ScientificCoefficient:
						this.SetCurrentNumberSection(NumberSection.ScientificCoefficientFraction);
						break;

					case NumberSection.ScientificExponent:
						this.SetCurrentNumberSection(NumberSection.ScientificExponentFraction);
						break;

					case NumberSection.Fraction:
					case NumberSection.GeneratedNumerator:
					case NumberSection.ScientificCoefficientFraction:
					case NumberSection.ScientificExponentFraction:
						break;

					default:
						Utilities.DebugFail("We should not encounter a decimal point in any other number section.");
						break;
				}
			}

			#endregion // OnDecimalSeparatorEncountered

			#region OnFractionEncountered

			public void OnFractionEncountered()
			{
				Debug.Assert(this.currentNumberSection == NumberSection.GeneratedNumerator, "We should have been in the numerator here.");
				this.SetCurrentNumberSection(NumberSection.GeneratedDenominator);
			}

			#endregion // OnFractionEncountered

			// MD 2/14/12 - 12.1 - Table Support
			#region OnFractionDigitUsed

			public void OnFractionDigitUsed()
			{
				this.fractionFormatDigitsUsed++;
			}

			#endregion // OnFractionDigitUsed

			#region OnLiteralEncountered

			internal void OnLiteralEncountered()
			{
				if (this.section.HasFractionSeparator &&
					this.section.GeneratedFractionUsesFullNumber == false &&
					this.currentNumberSection == NumberSection.Number)
				{
					this.SetCurrentNumberSection(NumberSection.GeneratedNumerator);
				}
			}

			#endregion // OnLiteralEncountered

			#region OnScientificNotationSeparatorEncountered

			internal void OnScientificNotationSeparatorEncountered()
			{
				Debug.Assert(
					this.currentNumberSection == NumberSection.ScientificCoefficient || 
					this.currentNumberSection == NumberSection.ScientificCoefficientFraction,
					"The scientific notation separator should be after the coefficient.");

				this.SetCurrentNumberSection(NumberSection.ScientificExponent);
			}

			#endregion // OnScientificNotationSeparatorEncountered

			#region Reduce

			private static void Reduce(double value, int limitDigits, out int numerator, out int denominator)
			{
				denominator = 1;
				numerator = (int)value;

				double z = value;
				int limit = (int)Math.Pow(10, limitDigits) - 1;
				int previousDenominator = 0;

				// MD 3/16/12 - TFS105094
				// We may need get a closer value with one of the fractions along the way and then move away from it,
				// so we should keep track of the values which produced the closest result.
				//while (value - ((double)numerator / denominator) != 0 && z != Math.Floor(z))
				double result = (double)numerator / denominator;
				double minDifference = Math.Abs(value - result);
				int minDifferenceDenominator = denominator;
				int minDifferenceNumerator = numerator;
				while (value - result != 0 && z != Math.Floor(z))
				{
					z = 1 / (z - Math.Floor(z));

					int newDenominator = denominator * (int)Math.Floor(z) + previousDenominator;

					if (newDenominator > limit)
					{
						// MD 3/16/12 - TFS105094
						// Just break here so we can use the code under the loop.
						//return;
						break;
					}

					previousDenominator = denominator;
					denominator = newDenominator;

					numerator = (int)ValueFormatter.Round(value * denominator);

					// MD 3/16/12 - TFS105094
					// If these values produce a fraction that is closer to the actual value, keep track of the current values.
					result = (double)numerator / denominator;
					double difference = Math.Abs(value - result);
					if (difference < minDifference)
					{
						minDifference = difference;
						minDifferenceDenominator = denominator;
						minDifferenceNumerator = numerator;
					}
				}

				// MD 3/16/12 - TFS105094
				// Use the values which produced the closet value.
				denominator = minDifferenceDenominator;
				numerator = minDifferenceNumerator;
			}

			#endregion // Reduce

			#region ResetCurrentDigitPosition

			public void ResetCurrentDigitPosition()
			{
				this.currentDigitPosition = 0;
			}

			#endregion // ResetCurrentDigitPosition

			#region SetCurrentNumberSection

			private void SetCurrentNumberSection(NumberSection numberSection)
			{
				this.currentNumberSection = numberSection;
				this.ResetCurrentDigitPosition();
			}

			#endregion // SetCurrentNumberSection

			#endregion // Methods

			#region Properties

			#region AbsoluteValue

			public double AbsoluteValue
			{
				get { return this.absoluteValue; }
			}

			#endregion // AbsoluteValue

			#region CurrentDigitPosition

			public int CurrentDigitPosition
			{
				get { return this.currentDigitPosition; }
				set { this.currentDigitPosition = value; }
			} 

			#endregion // CurrentDigitPosition

			#region CurrentNumberSection

			public NumberSection CurrentNumberSection
			{
				get { return this.currentNumberSection; }
			} 

			#endregion // CurrentNumberSection

			#region DateTime

			public DateTime? DateTime
			{
				get
				{
					if (this.dateTime.HasValue == false)
					{
						this.dateTime = UltraCalcValue.ExcelDateToDateTime(

section.Formatter.workbook,

							this.originalValue,
							// MD 2/14/12 - 12.1 - Table Support
							true, 
							true);
					}

					return this.dateTime;
				}
			} 

			#endregion // DateTime

			// MD 2/14/12 - 12.1 - Table Support
			#region FractionDigitsAllowedInVerbatimString

			public int FractionDigitsAllowedInVerbatimString
			{
				get { return this.fractionDigitsAllowedInVerbatimString; }
				set { this.fractionDigitsAllowedInVerbatimString = value; }
			}

			#endregion // FractionDigitsAllowedInVerbatimString

			// MD 2/14/12 - 12.1 - Table Support
			#region FractionDigitsUsedInVerbatimString

			public int FractionDigitsUsedInVerbatimString
			{
				get { return this.fractionDigitsUsedInVerbatimString; }
				set { this.fractionDigitsUsedInVerbatimString = value; }
			}

			#endregion // FractionDigitsUsedInVerbatimString

			// MD 2/14/12 - 12.1 - Table Support
			#region FractionFormatDigitsUsed

			public int FractionFormatDigitsUsed
			{
				get { return this.fractionFormatDigitsUsed; }
			}

			#endregion // FractionFormatDigitsUsed

			#region FractionPortionText

			public string FractionPortionText
			{
				get { return this.fractionPortionText; }
			} 

			#endregion // FractionPortionText

			#region GeneratedDenominator

			public int GeneratedDenominator
			{
				get { return this.generatedDenominator; }
			}

			#endregion // GeneratedDenominator

			#region GeneratedDenominatorIsLeftAligned

			public bool GeneratedDenominatorIsLeftAligned
			{
				get { return this.generatedDenominatorIsLeftAligned; }
				set { this.generatedDenominatorIsLeftAligned = value; }
			}

			#endregion // GeneratedDenominatorIsLeftAligned

			#region GeneratedDenominatorText

			public string GeneratedDenominatorText
			{
				get { return this.generatedDenominatorText; }
			} 

			#endregion // GeneratedDenominatorText

			#region GeneratedNumerator

			public int GeneratedNumerator
			{
				get { return this.generatedNumerator; }
			} 

			#endregion // GeneratedNumerator

			#region GeneratedNumeratorText

			public string GeneratedNumeratorText
			{
				get { return this.generatedNumeratorText; }
			}

			#endregion // GeneratedNumeratorText

			// MD 2/14/12 - 12.1 - Table Support
			#region OriginalValue

			public double OriginalValue
			{
				get { return this.originalValue; }
			}

			#endregion // OriginalValue

			#region NumberPortion

			public double NumberPortion
			{
				get { return this.numberPortion; }
			}

			#endregion // NumberPortion

			#region NumberPortionText

			public string NumberPortionText
			{
				get { return this.numberPortionText; }
			}

			#endregion // NumberPortionText

			#region ScientificExponent

			public int ScientificExponent
			{
				get { return this.scientificExponent; }
			}

			#endregion // ScientificExponent

			#region ScientificExponentText

			public string ScientificExponentText
			{
				get { return this.scientificExponentText; }
			}

			#endregion // ScientificExponentText

			#endregion // Properties
		}

		#endregion // FormatValueState class


		#region Part abtract class

		private abstract class Part
		{
			public abstract string GetText(FormatSection section, string text);
			public abstract string GetText(FormatSection section, FormatValueState state);
		}

		#endregion // Part abtract class

		#region LiteralFormatPart class

		[DebuggerDisplay("LiteralPart: '{text,nq}'")]
		private class LiteralFormatPart : Part
		{
			#region Member Variables

			private bool isRepeated;
			private string text;

			#endregion // Member Variables

			#region Constructor

			public LiteralFormatPart(string text, bool isRepeated)
			{
				this.text = text;
				this.isRepeated = isRepeated;
			}

			#endregion // Constructor

			#region Base Class Overrides

			public override string GetText(FormatSection section, string text)
			{
				if (this.isRepeated)
					return string.Empty;

				return this.text;
			}

			public override string GetText(FormatSection section, FormatValueState state)
			{
				state.OnLiteralEncountered();

				if (this.isRepeated)
					return string.Empty;

				return this.text;
			}

			#endregion // Base Class Overrides

			#region Properties

			// MD 2/14/12 - 12.1 - Table Support
			#region IsRepeated

			public bool IsRepeated
			{
				get { return this.isRepeated; }
			}

			#endregion // IsRepeated

			#region Text

			public string Text
			{
				get { return this.text; }
			}

			#endregion // Text

			#endregion // Properties
		}

		#endregion // LiteralFormatPart class

		#region WellKnownPart class

		[DebuggerDisplay("WellKnownPart: {type}")]
		private class WellKnownPart : Part
		{
			#region Member Variables

			private int repeatCount;
			private WellKnownPartType type;

			#endregion // Member Variables

			#region Constructor

			public WellKnownPart(WellKnownPartType type, int repeatCount)
			{
				this.type = type;
				this.repeatCount = repeatCount;
			}

			#endregion // Constructor

			#region Base Class Overrides

			public override string GetText(FormatSection section, string text)
			{
				switch (this.type)
				{
					case WellKnownPartType.GeneralString:
					case WellKnownPartType.VerbatimString:
						return text;

					case WellKnownPartType.Whitespace:
						return " ";

					default:
						Utilities.DebugFail("This part should not be in a text section.");
						return string.Empty;
				}
			}

			public override string GetText(FormatSection section, FormatValueState state)
			{
				DateTime dateTime = DateTime.MinValue;

				if (WellKnownPart.IsDateTime(this.type) &&
					this.type != WellKnownPartType.HoursTotal &&
					this.type != WellKnownPartType.MinutesTotal &&
					this.type != WellKnownPartType.SecondsTotal)
				{
					// MD 2/14/12 - 12.1 - Table Support
					// Return null if the value is negative and we need an actual date/time value and not totals.
					if (state.OriginalValue < 0)
						return null;

					if (state.DateTime.HasValue == false)
						return null;

					dateTime = state.DateTime.Value;
				}

				switch (this.type)
				{
					case WellKnownPartType.Whitespace:
						return " ";

					case WellKnownPartType.Percent:
						return section.Culture.NumberFormat.PercentSymbol;

					case WellKnownPartType.DecimalSeparator:
						{
							string totalText = string.Empty;
							if (state.CurrentNumberSection == NumberSection.Number)
							{
								if (1 <= state.AbsoluteValue && section.NumberDigitCount == 0)
									totalText += state.NumberPortionText;
							}
							else if (state.CurrentNumberSection == NumberSection.GeneratedNumerator)
							{
								if (section.GeneratedNumeratorDigitCount == 0)
									totalText += state.GeneratedNumeratorText;
							}

							totalText += section.Culture.NumberFormat.NumberDecimalSeparator;

							// This must be called at the end so we can get the correct value of IsInFractionPortion above.
							state.OnDecimalSeparatorEncountered();
							return totalText;
						}

					case WellKnownPartType.DigitOrEmpty:
					case WellKnownPartType.DigitOrZero:
					case WellKnownPartType.DigitOrWhitespace:
						{
							if (this.type == WellKnownPartType.DigitOrWhitespace &&
								state.GeneratedDenominatorIsLeftAligned == false &&
								state.CurrentNumberSection == NumberSection.GeneratedDenominator)
							{
								state.GeneratedDenominatorIsLeftAligned = true;
								state.ResetCurrentDigitPosition();
							}

							string text = this.GetDigitText(section, state);
							state.CurrentDigitPosition++;
							return text;
						}

					case WellKnownPartType.GroupShiftPlaceholder:
						return string.Empty;

					case WellKnownPartType.FractionSeparator:
						state.OnFractionEncountered();

						// MD 3/16/12 - TFS105094
						// If we shouldn't have a generated fraction, return a space for the fraction separator.
						if (section.GeneratedFractionIsOptional && state.GeneratedNumerator == 0)
							return " ";

						return "/";

					case WellKnownPartType.Scientific:
					case WellKnownPartType.ScientificWithSign:
						{
							state.OnScientificNotationSeparatorEncountered();

							string text = "E";
							if (state.ScientificExponent < 0 || this.type == WellKnownPartType.ScientificWithSign)
							{
								text += state.ScientificExponent < 0
									? section.Culture.NumberFormat.NegativeSign
									: section.Culture.NumberFormat.PositiveSign;
							}

							return text;
						}

					#region DateTime parts

					// MD 3/19/12 - TFS105157
					case WellKnownPartType.EraYearsShort:
						if (section.CalendarForErasResolved.Eras.Length == 1)
							goto case WellKnownPartType.YearsLong;

						return section.CultureFormatNumber(section.CalendarForErasResolved.GetYear(dateTime), 1);

					// MD 3/19/12 - TFS105157
					case WellKnownPartType.EraYearsLong:
						if (section.CalendarForErasResolved.Eras.Length == 1)
							goto case WellKnownPartType.YearsLong;

						return section.CultureFormatNumber(section.CalendarForErasResolved.GetYear(dateTime), 2);

					// MD 3/19/12 - TFS105157
					case WellKnownPartType.EraEnglishName:
					case WellKnownPartType.EraAbbreviatedName:
					case WellKnownPartType.EraName:
						{
							Calendar calendar = section.CalendarForErasResolved;
							if (calendar.Eras.Length == 1)
								return string.Empty;

							int era = calendar.GetEra(dateTime);

							DateTimeFormatInfo dateTimeFormat = (DateTimeFormatInfo)section.CultureForDatesResolved.DateTimeFormat.Clone();
							dateTimeFormat.Calendar = calendar;
							switch (this.type)
							{
								case WellKnownPartType.EraEnglishName:
									for (char testChar = 'A'; testChar <= 'Z'; testChar++)
									{
										string testEraName = testChar.ToString();
										if (dateTimeFormat.GetEra(testEraName) == era)
											return testEraName;
									}
									Utilities.DebugFail("Cannot find the english era name.");
									return string.Empty;

								case WellKnownPartType.EraAbbreviatedName:
									return dateTimeFormat.GetAbbreviatedEraName(era);

								case WellKnownPartType.EraName:
									return dateTimeFormat.GetEraName(era);

								default:
									Utilities.DebugFail("Unknown WellKnownPartType: " + this.type);
									goto case WellKnownPartType.EraName;
							}
						}

					case WellKnownPartType.YearsShort:
						// MD 3/19/12 - TFS105157
						// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
						//return dateTime.ToString("yy", section.CultureForDatesResolved);			
						return section.CultureFormatYear(dateTime, 2);

					case WellKnownPartType.YearsLong:
						// MD 3/19/12 - TFS105157
						// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
						//return dateTime.ToString("yyyy", section.CultureForDatesResolved);
						return section.CultureFormatYear(dateTime, 4);

					case WellKnownPartType.BuddhistYearsShort:
						// MD 3/19/12 - TFS105157
						// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
						//return dateTime.AddYears(543).ToString("yy", section.CultureForDatesResolved);
						// MD 3/26/12 - 12.1 - Table Support
						// We shouldn't change the date because the months/days may be different. Instead, use the Buddhist calendar.
						//return section.CultureFormatYear(dateTime.AddYears(543), 2);
						return section.CultureFormatYear(new ThaiBuddhistCalendar(), dateTime, 2);

					case WellKnownPartType.BuddhistYearsLong:
						// MD 3/19/12 - TFS105157
						// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
						//return dateTime.AddYears(543).ToString("yyyy", section.CultureForDatesResolved);
						// MD 3/26/12 - 12.1 - Table Support
						// We shouldn't change the date because the months/days may be different. Instead, use the Buddhist calendar.
						//return section.CultureFormatYear(dateTime.AddYears(543), 4);
						return section.CultureFormatYear(new ThaiBuddhistCalendar(), dateTime, 4);

					case WellKnownPartType.MonthsShort:
						// MD 3/19/12 - TFS105157
						// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
						//return dateTime.ToString("%M", section.CultureForDatesResolved);
						return section.CultureFormatMonth(dateTime, 1);

					case WellKnownPartType.MonthsLong:
						// MD 3/19/12 - TFS105157
						// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
						//return dateTime.ToString("MM", section.CultureForDatesResolved);
						return section.CultureFormatMonth(dateTime, 2);

					case WellKnownPartType.MonthShortDescription:
						return dateTime.ToString("MMM", section.CultureForDatesResolved);

					case WellKnownPartType.MonthLongDescription:
						return dateTime.ToString("MMMM", section.CultureForDatesResolved);

					case WellKnownPartType.MonthFirstLetter:
						return dateTime.ToString("MMMM", section.CultureForDatesResolved)[0].ToString();

					case WellKnownPartType.DaysShort:
						// MD 3/19/12 - TFS105157
						// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
						//return dateTime.ToString("%d", section.CultureForDatesResolved);
						return section.CultureFormatDay(dateTime, 1);

					case WellKnownPartType.DaysLong:
						// MD 3/19/12 - TFS105157
						// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
						//return dateTime.ToString("dd", section.CultureForDatesResolved);
						return section.CultureFormatDay(dateTime, 2);

					case WellKnownPartType.DayShortDescription:
						return dateTime.ToString("ddd", section.CultureForDatesResolved);

					case WellKnownPartType.DayLongDescription:
						return dateTime.ToString("dddd", section.CultureForDatesResolved);

					case WellKnownPartType.HoursShort:
					case WellKnownPartType.HoursLong:
						{
							// MD 3/19/12 - TFS105157
							//string format = this.type == WellKnownPartType.HoursShort ? "0" : "00";
							int requiredNumberOfDigits = this.type == WellKnownPartType.HoursShort ? 1 : 2;

							int hours = dateTime.TimeOfDay.Hours;

							// MD 3/15/12 - TFS104995
							// When we have an AM/PM part and the hours is 0, it should display as 12 AM, not 0 AM.
							//if (12 < hours && section.HasAMPMPart)
							//    hours -= 12;
							if (section.HasAMPMPart)
							{
								if (hours == 0)
									hours = 12;
								else if (12 < hours)
									hours -= 12;
							}

							// MD 3/19/12 - TFS105157
							// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
							//return hours.ToString(format);
							return section.CultureFormatNumber(hours, requiredNumberOfDigits);
						}

					case WellKnownPartType.HoursTotal:
					case WellKnownPartType.MinutesTotal:
					case WellKnownPartType.SecondsTotal:
						{
							// MD 2/27/12
							// Found while implementing 12.1 - Table Support
							// If no milliseconds are shown, we may need to round the seconds up.
							bool shouldRound = false;

							double multiplier;
							switch (this.type)
							{
								case WellKnownPartType.HoursTotal:
									multiplier = 24;
									break;

								case WellKnownPartType.MinutesTotal:
									multiplier = 1440;
									break;

								case WellKnownPartType.SecondsTotal:
									multiplier = 86400;

									// MD 2/27/12
									// Found while implementing 12.1 - Table Support
									// If no milliseconds are shown, we may need to round the seconds up.
									shouldRound = (section.HasMillisecondPart == false);

									break;

								default:
									Utilities.DebugFail("This shouldn't have happened.");
									goto case WellKnownPartType.HoursTotal;
							}

							// MD 2/27/12
							// Found while implementing 12.1 - Table Support
							// If no milliseconds are shown, we may need to round the seconds up.
							//double totalValue = Math.Floor(state.AbsoluteValue * multiplier);
							double totalValue = state.AbsoluteValue * multiplier;
							if (shouldRound)
								totalValue += 0.5;

							totalValue = Math.Floor(totalValue);

							// MD 3/19/12 - TFS105157
							// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
							//return totalValue.ToString(new string('0', this.repeatCount));
							return section.ConvertNumberTextToNativeDigits(totalValue.ToString(new string('0', this.repeatCount)));
						}

					case WellKnownPartType.MinutesShort:
						// MD 3/19/12 - TFS105157
						// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
						//return dateTime.TimeOfDay.Minutes.ToString();
						return section.CultureFormatNumber(dateTime.TimeOfDay.Minutes);

					case WellKnownPartType.MinutesLong:
						// MD 3/19/12 - TFS105157
						// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
						//return dateTime.TimeOfDay.Minutes.ToString("00");
						return section.CultureFormatNumber(dateTime.TimeOfDay.Minutes, 2);

					// MD 2/8/12 - 12.1 - Table Support
					// If no millisecond parts are present in the section, we need to round the seconds.
					//case WellKnownPartType.SecondsShort:
					//    return dateTime.TimeOfDay.Seconds.ToString();
					//
					//case WellKnownPartType.SecondsLong:
					//    return dateTime.TimeOfDay.Seconds.ToString("00");
					case WellKnownPartType.SecondsShort:
					case WellKnownPartType.SecondsLong:
						{
							int secondsResolved = dateTime.TimeOfDay.Seconds;
							if (section.HasMillisecondPart == false && 500 <= dateTime.TimeOfDay.Milliseconds)
								secondsResolved++;

							if (this.type == WellKnownPartType.SecondsShort)
								return section.CultureFormatNumber(secondsResolved);
							else
								return section.CultureFormatNumber(secondsResolved, 2);
						}

					case WellKnownPartType.Milliseconds1:
					case WellKnownPartType.Milliseconds2:
					case WellKnownPartType.Milliseconds3:
						{
							int shiftAmount;

							switch (this.type)
							{
								case WellKnownPartType.Milliseconds1:
									shiftAmount = 2;
									break;

								case WellKnownPartType.Milliseconds2:
									shiftAmount = 1;
									break;

								case WellKnownPartType.Milliseconds3:
									shiftAmount = 0;
									break;

								default:
									Utilities.DebugFail("Unexpected type.");
									return string.Empty;
							}

							int milliseconds = (int)ValueFormatter.Round(dateTime.TimeOfDay.Milliseconds / Math.Pow(10, shiftAmount));

							// MD 3/19/12 - TFS105157
							// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
							//return section.Culture.NumberFormat.NumberDecimalSeparator + milliseconds.ToString();
							return section.Culture.NumberFormat.NumberDecimalSeparator + section.CultureFormatNumber(milliseconds);
						}

					case WellKnownPartType.AMPMShortLL:
					case WellKnownPartType.AMPMShortLU:
					case WellKnownPartType.AMPMShortUL:
					case WellKnownPartType.AMPMShortUU:
						{
							bool isInAM = dateTime.TimeOfDay.Hours < 12;

							string text = dateTime.ToString("%t", section.CultureForDatesResolved);

							bool isLower;

							switch (this.type)
							{
								case WellKnownPartType.AMPMShortLL:
									isLower = true;
									break;

								case WellKnownPartType.AMPMShortLU:
									isLower = isInAM;
									break;

								case WellKnownPartType.AMPMShortUL:
									isLower = (isInAM == false);
									break;

								case WellKnownPartType.AMPMShortUU:
									isLower = false;
									break;

								default:
									Utilities.DebugFail("Unexpected type.");
									goto case WellKnownPartType.AMPMShortUU;
							}

							if(isLower)
								text = text.ToLower();

							return text;
						}

					case WellKnownPartType.AMPMLong:
						return dateTime.ToString("tt", section.CultureForDatesResolved);

					// MD 3/24/12 - TFS105636
					// Added support for the Chinese AM/PM format code.
					case WellKnownPartType.AMPMChinese:
						if (dateTime.TimeOfDay.Hours < 12)
							return "上午";

						return "下午";

					#endregion // DateTime parts

					case WellKnownPartType.GeneralString:
					case WellKnownPartType.VerbatimString:
						{
							// MD 2/8/12 - 12.1 - Table Support
							#region Moved

							//double value = state.AbsoluteValue;
							//string text = value.ToString();

							//if (text.Length <= 11)
							//    return text;

							//if (value < 0.0001 || 99999999999 < value)
							//{
							//    int exponent = (int)Math.Floor(Math.Log10(value));
							//    string exponentText = Math.Abs(exponent).ToString("00");
							//    int roundingDigits = Math.Max(0, 7 - exponentText.Length);
							//    double coefficient = value / Math.Pow(10, exponent);
							//    string coefficientText = Math.Round(coefficient, roundingDigits).ToString();
							//    string exponentSign = exponent < 0 ? section.Culture.NumberFormat.NegativeSign : section.Culture.NumberFormat.PositiveSign;
							//    return coefficientText + "E" + exponentSign + exponentText;
							//}
							//else
							//{
							//    if (value < 1)
							//        return ValueFormatter.Round(value, 9).ToString();

							//    int significantDigitsBeforeDecimal = (int)Math.Floor(Math.Log10(value)) + 1;
							//    int roundingDigits = Math.Max(0, 10 - significantDigitsBeforeDecimal);
							//    return ValueFormatter.Round(value, roundingDigits).ToString();
							//}

							#endregion // Moved
							return section.GetVerbatimString(state);
						}

					default:
						Utilities.DebugFail("This part should not be in a number or date section.");
						return string.Empty;
				}
			}

			#endregion // Base Class Overrides

			#region Methods

			#region ConvertMonthPartToMinutePart

			public void ConvertMonthPartToMinutePart()
			{
				switch (this.type)
				{
					case WellKnownPartType.MonthsShort:
						this.type = WellKnownPartType.MinutesShort;
						break;

					case WellKnownPartType.MonthFirstLetter:
					case WellKnownPartType.MonthsLong:
					case WellKnownPartType.MonthLongDescription:
					case WellKnownPartType.MonthShortDescription:
						this.type = WellKnownPartType.MinutesLong;
						break;
				}
			}

			#endregion // ConvertMonthPartToMinutePart

			#region GetDigitText

			private string GetDigitText(FormatSection section, FormatValueState state)
			{
				string text = this.GetDigitTextHelper(section, state);

				if (text != null)
					return text;

				switch (this.type)
				{
					case WellKnownPartType.DigitOrEmpty:
						return string.Empty;

					case WellKnownPartType.DigitOrZero:
						// MD 3/19/12 - TFS105157
						// The .NET number formatting logic does not use the native digits, so we need to generate the string manually.
						//text = 0.ToString();
						text = section.CultureFormatNumber(0, 1);

						if (state.CurrentNumberSection == NumberSection.Number)
							section.InsertGroupSeparators(ref text, state.CurrentDigitPosition, state.NumberPortionText);

						return text;

					case WellKnownPartType.DigitOrWhitespace:
						text = " ";

						if (state.CurrentNumberSection == NumberSection.Number)
							section.InsertGroupSeparators(ref text, state.CurrentDigitPosition, state.NumberPortionText, " ");

						return text;

					default:
						Utilities.DebugFail("Unknown part type.");
						return string.Empty;
				}
			}

			private string GetDigitTextHelper(FormatSection section, FormatValueState state)
			{
				switch (state.CurrentNumberSection)
				{
					case NumberSection.Number:
					case NumberSection.ScientificCoefficient:
						return this.GetDigitTextHelper(section, state, state.NumberPortion, state.NumberPortionText, section.NumberDigitCount, false);

					case NumberSection.Fraction:
					case NumberSection.ScientificCoefficientFraction:
						// MD 2/14/12 - 12.1 - Table Support
						// Keep track of when a digit character is used (actually displays a digit(s)).
						//return this.GetDigitTextHelper(section, state, null, state.FractionPortionText, section.FractionDigitCount, true);
						string result = this.GetDigitTextHelper(section, state, null, state.FractionPortionText, section.FractionDigitCount, true);
						if(result != null)
							state.OnFractionDigitUsed();

						return result;

					case NumberSection.ScientificExponent:
						return this.GetDigitTextHelper(section, state, state.ScientificExponent, state.ScientificExponentText, section.ScientificExponentDigitCount, false);

					case NumberSection.ScientificExponentFraction:
						return null;

					case NumberSection.GeneratedDenominator:

						// MD 3/16/12 - TFS105094
						// If the generated fraction is not needed, return at least one space and then all characters as if 0 was present.
						if (section.GeneratedFractionIsOptional && state.GeneratedNumerator == 0)
						{
							if (state.CurrentDigitPosition == 0)
								return " ";

							return this.GetDigitTextHelper(section, state, 0, new string(' ', state.GeneratedDenominatorText.Length), section.GeneratedDenominatorDigitCount, state.GeneratedDenominatorIsLeftAligned);
						}

						return this.GetDigitTextHelper(section, state, state.GeneratedDenominator, state.GeneratedDenominatorText, section.GeneratedDenominatorDigitCount, state.GeneratedDenominatorIsLeftAligned);

					case NumberSection.GeneratedNumerator:

						// MD 3/16/12 - TFS105094
						// If the generated fraction is not needed, return all characters as if 0 was present.
						if (section.GeneratedFractionIsOptional && state.GeneratedNumerator == 0)
							return this.GetDigitTextHelper(section, state, 0, new string(' ', state.GeneratedNumeratorText.Length), section.GeneratedNumeratorDigitCount, false);

						return this.GetDigitTextHelper(section, state, state.GeneratedNumerator, state.GeneratedNumeratorText, section.GeneratedNumeratorDigitCount, false);

					default:
						Utilities.DebugFail("Unknown number section type.");
						return string.Empty;
				}
			}

			private string GetDigitTextHelper(FormatSection section, FormatValueState state, double? numberSectionValue, string numberSectionText, int digitCount, bool isLeftAligned)
			{
				if (numberSectionText == null)
				{
					Utilities.DebugFail("The numberSectionText should not be null.");
					return null;
				}

				if (isLeftAligned)
				{
					if (state.CurrentDigitPosition < numberSectionText.Length)
						return numberSectionText[state.CurrentDigitPosition].ToString();

					return null;
				}

				Debug.Assert(numberSectionValue.HasValue, "We should have a value here.");

				// MD 3/22/12 - TFS105610
				// If there is a number followed by a generated fraction, and the number portion is 0, it is always displayed, even
				// if the format character is a '#' or '?'.
				//if (numberSectionValue.HasValue && numberSectionValue.Value == 0)
				if (numberSectionValue.HasValue && numberSectionValue.Value == 0 &&
					(section.HasFractionSeparator == false || state.CurrentNumberSection != NumberSection.Number))
				{
					switch (this.type)
					{
						case WellKnownPartType.DigitOrEmpty:
							return string.Empty;

						case WellKnownPartType.DigitOrWhitespace:
							return " ";
					}
				}

				string text = null;

				int difference = numberSectionText.Length - digitCount;
				if (difference >= 0)
				{
					// The mask for the number portion is lined up or shorter than the number text...

					if (state.CurrentDigitPosition == 0)
					{
						// The first digit should contain the text in it's digit position as well as everything before it.
						text = numberSectionText.Substring(0, difference + 1);
					}
					else
					{
						text = numberSectionText[difference + state.CurrentDigitPosition].ToString();
					}
				}
				else
				{
					int resolvedDigitPosition = state.CurrentDigitPosition + difference;

					if (resolvedDigitPosition >= 0)
						text = numberSectionText[resolvedDigitPosition].ToString();
				}

				if (text != null)
				{
					if (state.CurrentNumberSection == NumberSection.Number ||
						state.CurrentNumberSection == NumberSection.ScientificCoefficient)
					{
						section.InsertGroupSeparators(ref text, state.CurrentDigitPosition, numberSectionText);
					}
				}

				return text;
			}

			#endregion // GetDigitText

			#region IsDateTime

			public static bool IsDateTime(WellKnownPartType type)
			{
				switch (type)
				{
					case WellKnownPartType.AMPMLong:
					case WellKnownPartType.AMPMShortLL:
					case WellKnownPartType.AMPMShortLU:
					case WellKnownPartType.AMPMShortUL:
					case WellKnownPartType.AMPMShortUU:
					// MD 3/24/12 - TFS105636
					// Added support for the Chinese AM/PM format code.
					case WellKnownPartType.AMPMChinese:
					case WellKnownPartType.BuddhistYearsLong:
					case WellKnownPartType.BuddhistYearsShort:
					case WellKnownPartType.DaysLong:
					case WellKnownPartType.DayLongDescription:
					case WellKnownPartType.DaysShort:
					case WellKnownPartType.DayShortDescription:
					case WellKnownPartType.HoursLong:
					case WellKnownPartType.HoursShort:
					case WellKnownPartType.HoursTotal:
					case WellKnownPartType.Milliseconds1:
					case WellKnownPartType.Milliseconds2:
					case WellKnownPartType.Milliseconds3:
					case WellKnownPartType.MinutesLong:
					case WellKnownPartType.MinutesShort:
					case WellKnownPartType.MinutesTotal:
					case WellKnownPartType.MonthFirstLetter:
					case WellKnownPartType.MonthsLong:
					case WellKnownPartType.MonthLongDescription:
					case WellKnownPartType.MonthsShort:
					case WellKnownPartType.MonthShortDescription:
					case WellKnownPartType.SecondsLong:
					case WellKnownPartType.SecondsShort:
					case WellKnownPartType.SecondsTotal:
					case WellKnownPartType.YearsLong:
					case WellKnownPartType.YearsShort:
					// MD 3/19/12 - TFS105157
					case WellKnownPartType.EraYearsShort:
					case WellKnownPartType.EraYearsLong:
					case WellKnownPartType.EraEnglishName:
					case WellKnownPartType.EraAbbreviatedName:
					case WellKnownPartType.EraName:
						return true;
				}

				return false;
			}

			#endregion // IsDateTime

			#region IsDigit

			public static bool IsDigit(WellKnownPartType type)
			{
				switch (type)
				{
					case WellKnownPartType.DigitOrEmpty:
					case WellKnownPartType.DigitOrZero:
					case WellKnownPartType.DigitOrWhitespace:
						return true;
				}

				return false;
			}

			#endregion // IsDigit

			#region IsHour

			public static bool IsHour(WellKnownPartType type)
			{
				switch (type)
				{
					case WellKnownPartType.HoursShort:
					case WellKnownPartType.HoursLong:
					case WellKnownPartType.HoursTotal:
						return true;
				}

				return false;
			}

			#endregion // IsHour

			#endregion // Methods

			#region Properties

			#region Type

			public WellKnownPartType Type
			{
				get { return this.type; }
			}

			#endregion // Type

			#endregion // Properties
		}

		#endregion // WellKnownPart class


		#region CompareOperator enum

		private enum CompareOperator
		{
			// MD 2/14/12 - 12.1 - Table Support
			AnyValue,

			Equals,
			NotEquals,
			LessThan,
			LessThanOrEquals,
			GreaterThan,
			GreaterThanOrEquals,
		}

		#endregion // CompareOperator enum

		#region NumberSection enum

		private enum NumberSection
		{
			Number,
			Fraction,
			GeneratedNumerator,
			GeneratedDenominator,
			ScientificCoefficient,
			ScientificCoefficientFraction,
			ScientificExponent,
			ScientificExponentFraction,
		}

		#endregion // NumberSection enum

		#region SectionType enum

		internal enum SectionType
		{
			Default = 0,
			Date,
			Number,
			Text,
		}

		#endregion // SectionType enum

		#region WellKnownPartType enum

		private enum WellKnownPartType
		{
			Whitespace,

			// Text
			GeneralString,
			VerbatimString,

			// Number
			DecimalSeparator,
			DigitOrEmpty,
			DigitOrZero,
			DigitOrWhitespace,
			GroupShiftPlaceholder,
			Percent,
			FractionSeparator,
			Scientific,
			ScientificWithSign,

			// Time
			AMPMShortLL,
			AMPMShortLU,
			AMPMShortUL,
			AMPMShortUU,
			AMPMLong,
			// MD 3/24/12 - TFS105636
			// Added support for the Chinese AM/PM format code.
			AMPMChinese,
			HoursShort,
			HoursLong,
			HoursTotal,
			Milliseconds1,
			Milliseconds2,
			Milliseconds3,
			MinutesShort,
			MinutesLong,
			MinutesTotal,
			SecondsShort,
			SecondsLong,
			SecondsTotal,

			// Date
			BuddhistYearsShort,
			BuddhistYearsLong,
			DaysShort,
			DaysLong,
			DayShortDescription,
			DayLongDescription,
			MonthsShort,
			MonthsLong,
			MonthShortDescription,
			MonthLongDescription,
			MonthFirstLetter,
			YearsShort,
			YearsLong,
			// MD 3/19/12 - TFS105157
			EraYearsShort,
			EraYearsLong,
			EraEnglishName,
			EraAbbreviatedName,
			EraName,
		}

		#endregion // WellKnownPartType enum


		// MD 2/14/12 - 12.1 - Table Support

		#region GetTextWidthHelper class

		private class GetTextWidthHelper : IDisposable
		{






			private Bitmap bitmap;
			private Font font;
			private Graphics grfx;


			public GetTextWidthHelper(WorksheetCellFormatData cellFormat)
			{




				this.font = WorkbookFontData.CreateFont(
					cellFormat.FontNameResolved,
					cellFormat.FontHeightResolved,
					cellFormat.FontBoldResolved,
					cellFormat.FontItalicResolved,
					cellFormat.FontUnderlineStyleResolved,
					cellFormat.FontStrikeoutResolved);

				this.bitmap = new Bitmap(1, 1);
				this.grfx = Graphics.FromImage(bitmap);

			}

			public int GetWidth(FormatSection section, string text)
			{
				// MD 3/22/12 - TFS105610
				// Spaces seem to measure like '0' characters in number sections.
				if (section != null && section.SectionType == SectionType.Number)
					text = text.Replace(' ', '0');





				return TextRenderer.MeasureText(this.grfx, text, this.font, Size.Empty, TextFormatFlags.NoPadding).Width;

			}



#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)


			public void Dispose()
			{

				if (font != null)
					font.Dispose();

				if (grfx != null)
					grfx.Dispose();

				if (bitmap != null)
					bitmap.Dispose(); 

			}
		}

		#endregion // GetTextWidthHelper class

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