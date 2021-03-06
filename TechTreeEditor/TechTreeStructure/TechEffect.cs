﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace TechTreeEditor.TechTreeStructure
{
	/// <summary>
	/// Definiert einen Technologie-Effekt.
	/// </summary>
	public class TechEffect
	{
		#region Variablen

		/// <summary>
		/// Der Typ des Effekts.
		/// </summary>
		public EffectType Type { get; set; }

		/// <summary>
		/// Das Grundelement des Effekts.
		/// </summary>
		public TechTreeElement Element { get; set; }

		/// <summary>
		/// Filterausdruck für die Grundelemente des Effekts. Nur benutzt, wenn das Grundelement nicht definiert und keine Klasse gesetzt ist.
		/// </summary>
		public string ElementExpression { get; set; } = "";

		/// <summary>
		/// Das Zielelement eines Upgrade-Effekts.
		/// </summary>
		public TechTreeElement DestinationElement { get; set; }

		/// <summary>
		/// Der Modus des Effekts.
		/// </summary>
		public EffectMode Mode { get; set; }

		/// <summary>
		/// Die ID der betroffenen Einheiten-Klasse.
		/// </summary>
		public short ClassID { get; set; } = -1;

		/// <summary>
		/// Der zu ändernde Parameter.
		/// </summary>
		public short ParameterID { get; set; }

		/// <summary>
		/// Der anzuwendende Wert.
		/// </summary>
		public float Value { get; set; }

		/// <summary>
		/// Die gecachten Attribute.
		/// </summary>
		private static Dictionary<int, string> _attributes = null;

		/// <summary>
		/// Die gecachten Einheiten-Klassen.
		/// </summary>
		private static string[] _classes = null;

		/// <summary>
		/// Die gecachten Rüstungs-Klassen.
		/// </summary>
		private static string[] _armourClasses = null;

		/// <summary>
		/// Die gecachten Ressourcen-Typen.
		/// </summary>
		private static string[] _resourceTypes = null;

		#endregion Variablen

		#region Funktionen

		/// <summary>
		/// Erstellt einen neuen Effekt.
		/// </summary>
		public TechEffect()
		{
		}

		/// <summary>
		/// Erstellt einen neuen Effekt aus dem angegebenen XElement.
		/// </summary>
		/// <param name="element">Das XElement, aus dem das Element erstellt werden soll.</param>
		/// <param name="elementIDs">Die IDs der Baumelemente.</param>
		public TechEffect(XElement element, Dictionary<int, TechTreeElement> elementIDs)
		{
			// Lese-Funktion aufrufen
			FromXml(element, elementIDs);
		}

		/// <summary>
		/// Konvertiert die enthaltenen Daten in ein XML-Format und schreibt sie in den übergebenen XmlWriter.
		/// </summary>
		/// <param name="writer">Der XmlWriter, in den die Daten geschrieben werden sollen.</param>
		/// <param name="elementIDs">Die Liste mit den den TechTree-Elementen zugeordneten IDs.</param>
		public void ToXml(XmlWriter writer, Dictionary<TechTreeElement, int> elementIDs)
		{
			// Element-Anfangstag schreiben
			writer.WriteStartElement("effect");
			{
				// Elementtyp schreiben
				writer.WriteElementNumber("type", (byte)Type);

				// Grundelement schreiben
				if(Element != null)
					writer.WriteElementNumber("elem", elementIDs[Element]);
				writer.WriteElementString("elemfilter", ElementExpression);

				// Zielelement schreiben
				if(DestinationElement != null)
					writer.WriteElementNumber("delem", elementIDs[DestinationElement]);

				// Modus schreiben
				writer.WriteElementNumber("mode", (short)Mode);

				// Klassen-ID schreiben
				writer.WriteElementNumber("class", ClassID);

				// Parameter-ID schreiben
				writer.WriteElementNumber("param", ParameterID);

				// Wert schreiben
				writer.WriteElementNumber("value", Value);
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Liest das aktuelle Element aus dem angegebenen XElement.
		/// </summary>
		/// <param name="element">Das XElement, aus dem das Element erstellt werden soll.</param>
		/// <param name="elementIDs">Die IDs der Baumelemente.</param>
		public void FromXml(XElement element, Dictionary<int, TechTreeElement> elementIDs)
		{
			// Typ lesen
			Type = (EffectType)(uint)element.Element("type"); // Ein byte-Cast geht hier scheinbar nicht

			// Grundelement lesen
			if(element.Element("elem") != null)
				Element = elementIDs[(int)element.Element("elem")];
			ElementExpression = (string)element.Element("elemfilter");

			// Zielelement lesen
			if(element.Element("delem") != null)
				DestinationElement = elementIDs[(int)element.Element("delem")];

			// Modus lesen
			Mode = (EffectMode)(short)element.Element("mode");

			// Klassen-ID lesen
			ClassID = (short)element.Element("class");

			// Parameter-ID lesen
			ParameterID = (short)element.Element("param");

			// Wert lesen
			Value = (float)element.Element("value");
		}

		/// <summary>
		/// Gibt eine String-Repräsentation dieses Objekts zurück.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			// Ggf. Nachschlag-Listen erstellen
			if(_attributes == null)
			{
				_attributes = new Dictionary<int, string>();
				foreach(string a in Strings.Attributes.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
					_attributes.Add(int.Parse(a.Substring(0, a.IndexOf(':'))), a.Substring(a.IndexOf(':') + 2));
			}
			if(_classes == null)
				_classes = Strings.ClassNames.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			if(_armourClasses == null)
				_armourClasses = Strings.ArmourClasses.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			if(_resourceTypes == null)
				_resourceTypes = Strings.ResourceTypes.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

			// Nach Effekt-Typ vorgehen
			try
			{
				switch(Type)
				{
					case EffectType.AttributeSet:
						if(ParameterID == 8)
							return string.Format(Strings.TechEffect_ToString_AttributeSet_Armour, (Element != null ? Strings.TechEffect_ToString_Part_Unit + " '" + Element.Name + "'" : Strings.TechEffect_ToString_Part_Class + " '" + _classes[ClassID] + "'"), ((short)Value & 0xFF), _armourClasses[((short)Value >> 8)]);
						else if(ParameterID == 9)
							return string.Format(Strings.TechEffect_ToString_AttributeSet_Attack, (Element != null ? Strings.TechEffect_ToString_Part_Unit + " '" + Element.Name + "'" : Strings.TechEffect_ToString_Part_Class + " '" + _classes[ClassID] + "'"), ((short)Value & 0xFF), _armourClasses[((short)Value >> 8)]);
						else if(_attributes.ContainsKey(ParameterID) && (Element != null || ClassID >= 0))
							return string.Format(Strings.TechEffect_ToString_AttributeSet_Else, _attributes[ParameterID], (Element != null ? Strings.TechEffect_ToString_Part_Unit + " '" + Element.Name + "'" : Strings.TechEffect_ToString_Part_Class + " '" + _classes[ClassID] + "'"), Value);
						else
							return "[?]";

					case EffectType.ResourceSetPM:
						return string.Format(Strings.TechEffect_ToString_ResourceSetPM, _resourceTypes[ParameterID], (Mode == EffectMode.PM_Enable ? "+" : "="), Value);

					case EffectType.UnitEnableDisable:
						return string.Format(Strings.TechEffect_ToString_UnitEnableDisable, (Mode == EffectMode.PM_Enable ? Strings.TechEffect_ToString_Part_Enable : Strings.TechEffect_ToString_Part_Disable), (Element != null ? Element.Name : "?"));

					case EffectType.UnitUpgrade:
						return string.Format(Strings.TechEffect_ToString_UnitUpgrade, (Element != null ? Element.Name : "?"), (DestinationElement != null ? DestinationElement.Name : "?"));

					case EffectType.AttributePM:
						if(ParameterID == 8)
							return string.Format(Strings.TechEffect_ToString_AttributePM_Armour, (Element != null ? Strings.TechEffect_ToString_Part_Unit + " '" + Element.Name + "'" : Strings.TechEffect_ToString_Part_Class + " '" + _classes[ClassID] + "'"), ((short)Value & 0xFF), _armourClasses[((short)Value >> 8)]);
						else if(ParameterID == 9)
							return string.Format(Strings.TechEffect_ToString_AttributePM_Attack, (Element != null ? Strings.TechEffect_ToString_Part_Unit + " '" + Element.Name + "'" : Strings.TechEffect_ToString_Part_Class + " '" + _classes[ClassID] + "'"), ((short)Value & 0xFF), _armourClasses[((short)Value >> 8)]);
						else if(_attributes.ContainsKey(ParameterID) && (Element != null || ClassID >= 0))
							return string.Format(Strings.TechEffect_ToString_AttributePM_Else, _attributes[ParameterID], (Element != null ? Strings.TechEffect_ToString_Part_Unit + " '" + Element.Name + "'" : Strings.TechEffect_ToString_Part_Class + " '" + _classes[ClassID] + "'"), Value);
						else
							return "[?]";

					case EffectType.AttributeMult:
						if(ParameterID == 8)
							return string.Format(Strings.TechEffect_ToString_AttributeMult_Armour, (Element != null ? Strings.TechEffect_ToString_Part_Unit + " '" + Element.Name + "'" : Strings.TechEffect_ToString_Part_Class + " '" + _classes[ClassID] + "'"), ((short)Value & 0xFF), _armourClasses[((short)Value >> 8)]);
						else if(ParameterID == 9)
							return string.Format(Strings.TechEffect_ToString_AttributeMult_Attack, (Element != null ? Strings.TechEffect_ToString_Part_Unit + " '" + Element.Name + "'" : Strings.TechEffect_ToString_Part_Class + " '" + _classes[ClassID] + "'"), ((short)Value & 0xFF), _armourClasses[((short)Value >> 8)]);
						else if(_attributes.ContainsKey(ParameterID) && (Element != null || ClassID >= 0))
							return string.Format(Strings.TechEffect_ToString_AttributeMult_Else, _attributes[ParameterID], (Element != null ? Strings.TechEffect_ToString_Part_Unit + " '" + Element.Name + "'" : Strings.TechEffect_ToString_Part_Class + " '" + _classes[ClassID] + "'"), Value);
						else
							return "[?]";

					case EffectType.ResourceMult:
						return string.Format(Strings.TechEffect_ToString_ResourceMult, _resourceTypes[ParameterID], Value);

					case EffectType.ResearchCostSetPM:
						return string.Format(Strings.TechEffect_ToString_ResearchCostSetPM, _resourceTypes[ParameterID], (Element != null ? Element.Name : "?"), (Mode == EffectMode.PM_Enable ? "+" : "="), Value);

					case EffectType.ResearchDisable:
						return string.Format(Strings.TechEffect_ToString_ResearchDisable, (Element != null ? Element.Name : "?"));

					case EffectType.ResearchTimeSetPM:
						return string.Format(Strings.TechEffect_ToString_ResearchTimeSetPM, (Element != null ? Element.Name : "?"), (Mode == EffectMode.PM_Enable ? "+" : "="), Value);
				}
				return "?";
			}
			catch(IndexOutOfRangeException)
			{
				return "?";
			}
			catch(KeyNotFoundException)
			{
				return "?";
			}
		}

		/// <summary>
		/// Gibt eine Kopie dieses Effekts zurück.
		/// </summary>
		/// <returns></returns>
		public TechEffect Clone()
		{
			// Kopieren
			TechEffect copy = (TechEffect)MemberwiseClone();
			copy.ElementExpression = string.Copy(ElementExpression);
			return copy;
		}

		#endregion Funktionen

		#region Enumerationen

		/// <summary>
		/// Definiert die verschiedenen möglichen Effekt-Typen.
		/// </summary>
		public enum EffectType : byte
		{
			AttributeSet = 0,
			ResourceSetPM = 1,
			UnitEnableDisable = 2,
			UnitUpgrade = 3,
			AttributePM = 4,
			AttributeMult = 5,
			ResourceMult = 6,
			ResearchCostSetPM = 101,
			ResearchDisable = 102,
			ResearchTimeSetPM = 103,
			ResearchCostMult = 201,
			ResearchTimeMult = 202,
		}

		/// <summary>
		/// Der Effekt-Modus.
		/// </summary>
		public enum EffectMode : short
		{
			Set_Disable = 0,
			PM_Enable = 1
		}

		#endregion Enumerationen

		#region Unterklassen

		/// <summary>
		/// Erlaubt das Parsen und Auswerten eines Element-Filterausdrucks.
		/// </summary>
		public class ExpressionEvaluator
		{
			private static Regex _expressionStringParseRegex = new Regex("([A-Za-z\\.]+) *([\\<\\>\\=]+) *(.+)", RegexOptions.Compiled);
			private Func<TechTreeElement, TechTreeFile, bool> _expression = null;
			private bool _isUnitExpression = false;

			public ExpressionEvaluator(string expressionString, bool isUnit)
			{
				// Allgemeine Eigenschaften verfügbar machen
				ParameterExpression projectFileParam = Expression.Parameter(typeof(TechTreeFile));
				ParameterExpression elementParam = Expression.Parameter(typeof(TechTreeElement));
				Expression ageProperty = Expression.Property(elementParam, nameof(TechTreeElement.Age));

				// Einheitenspezifische Eigenschaften abrufen
				_isUnitExpression = isUnit;
				Expression classProperty = null;
				if(isUnit)
				{
					// Einheiten-Daten abrufen
					Expression unitDataMember = Expression.PropertyOrField(Expression.Convert(elementParam, typeof(TechTreeUnit)), nameof(TechTreeUnit.DATUnit));

					// Gewünschte Eigenschaften extrahieren
					classProperty = Expression.Convert(Expression.PropertyOrField(unitDataMember, nameof(GenieLibrary.DataElements.Civ.Unit.Class)), typeof(int));
				}

				// Nach AND-Blöcken aufteilen
				Expression expression = Expression.Constant(true, typeof(bool));
				foreach(string andOperand in expressionString.Split(new string[] { " and ", "&&" }, StringSplitOptions.RemoveEmptyEntries))
				{
					// Ausdruck zerlegen
					var andOperandParts = _expressionStringParseRegex.Match(andOperand.Trim());
					if(andOperandParts.Groups.Count != 4)
						throw new FormatException("Invalid count of operand parameters. Expected <name> <relation operator> <value>.");

					// Name
					Expression nameExpr = null;
					switch(andOperandParts.Groups[1].Value.ToLower())
					{
						case "age":
							nameExpr = ageProperty;
							break;
						case "class":
							if(isUnit)
								nameExpr = classProperty;
							else
								throw new FormatException($"The 'Class' filter can only be used with units.");
							break;
						case "parent.id":
							Expression getElementParentExpr = Expression.Call(projectFileParam, typeof(TechTreeFile).GetMethod(nameof(TechTreeFile.GetElementParent)), elementParam);
							nameExpr = Expression.Condition(Expression.Equal(getElementParentExpr, elementParam), Expression.Constant(-1, typeof(int)), Expression.Property(getElementParentExpr, nameof(TechTreeElement.ID)));
							break;
						default:
							throw new FormatException($"Unknown name '{andOperandParts.Groups[1].Value}'.");
					}

					// Wert
					Expression valExpr = null;
					if(int.TryParse(andOperandParts.Groups[3].Value, out int val))
						valExpr = Expression.Constant(val, typeof(int));
					else
						throw new FormatException($"Unknown value type.");

					// Relation
					Expression relExpr = null;
					switch(andOperandParts.Groups[2].Value)
					{
						case "==":
							relExpr = Expression.Equal(nameExpr, valExpr);
							break;
						case "<>":
							relExpr = Expression.NotEqual(nameExpr, valExpr);
							break;
						case "<=":
							relExpr = Expression.LessThanOrEqual(nameExpr, valExpr);
							break;
						case "<":
							relExpr = Expression.LessThan(nameExpr, valExpr);
							break;
						case ">=":
							relExpr = Expression.GreaterThanOrEqual(nameExpr, valExpr);
							break;
						case ">":
							relExpr = Expression.GreaterThan(nameExpr, valExpr);
							break;
						default:
							throw new FormatException($"Unknown relation operator '{andOperandParts.Groups[2].Value}'.");
					}

					// Teilausdruck mit vorhandenem ver-UND-en
					expression = Expression.AndAlso(expression, relExpr);
				}

				// In kompilierten Lambda umwandeln und intern speichern
				_expression = Expression.Lambda<Func<TechTreeElement, TechTreeFile, bool>>(expression, elementParam, projectFileParam).Compile();
			}

			/// <summary>
			/// Wertet den Filterausdruck für das gegebene Element aus.
			/// </summary>
			/// <param name="element">Das zu überprüfende Element. Der Typ wird automatisch anhand des im Konstruktor übergebenen Parameters geprüft.</param>
			/// <param name="projectFile">Die Projektdaten für weitere Checks.</param>
			/// <returns></returns>
			public bool CheckElement(TechTreeElement element, TechTreeFile projectFile)
				=> (_isUnitExpression != element is TechTreeResearch) ? _expression(element, projectFile) : false;
		}

		#endregion
	}
}