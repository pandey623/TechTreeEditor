﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using X2AddOnTechTreeEditor.TechTreeStructure;

namespace X2AddOnTechTreeEditor
{
	public partial class EditUnitAttributeForm : Form
	{
		#region Variablen

		/// <summary>
		/// Die referenzierte Baum-Einheit.
		/// </summary>
		TechTreeUnit _treeUnit;

		/// <summary>
		/// Der Einheiten-Manager zum korrekten Kopieren der veränderten Attribute.
		/// </summary>
		GenieUnitManager _unitManager;

		/// <summary>
		/// Die aktuelle Projektdatei.
		/// </summary>
		TechTreeFile _projectFile;

		/// <summary>
		/// Gibt an, ob das Formular komplett geladen ist.
		/// </summary>
		bool _formLoaded = false;

		#endregion

		#region Funktionen

		/// <summary>
		/// Konstruktor.
		/// </summary>
		public EditUnitAttributeForm()
		{
			// Steuerelemente laden
			InitializeComponent();
		}

		/// <summary>
		/// Erstellt ein neues Attribut-Formular basierend auf der gegebenen Einheit.
		/// </summary>
		/// <param name="projectFile">Die aktuelle Projektdatei.</param>
		/// <param name="unit">Die referenzierte Baum-Einheit.</param>
		/// <param name="unitManager">Der Einheiten-Manager zum korrekten Kopieren der veränderten Attribute.</param>
		public EditUnitAttributeForm(TechTreeFile projectFile, TechTreeUnit treeUnit, GenieUnitManager unitManager)
			: this()
		{
			// Parameter speichern
			_projectFile = projectFile;
			_treeUnit = treeUnit;
			_unitManager = unitManager;
			
			// Einheiten-Änderungen sperren, damit die Zuweisungen nicht alle Auto-Kopier-Einheiten überschreiben
			_unitManager.Lock();

			// Ausgewählte Einheit abrufen
			GenieLibrary.DataElements.Civ.Unit unit = unitManager.SelectedUnit;

			// Grafik-Liste erstellen
			GenieLibrary.DataElements.Graphic emptyG = new GenieLibrary.DataElements.Graphic() { ID = -1 };
			List<GenieLibrary.DataElements.Graphic> graphicList = _projectFile.BasicGenieFile.Graphics.Values.ToList();
			graphicList.Add(emptyG);
			graphicList.Sort((x, y) => x.ID.CompareTo(y.ID));
			List<object> tmpGrList = graphicList.Cast<object>().ToList();

			// Standard-Tab
			{
				// DLL-Werte einfügen
				_dllNameField.ProjectFile = _projectFile;
				_dllNameField.Value = unit.LanguageDLLName;
				_dllDescriptionField.ProjectFile = _projectFile;
				_dllDescriptionField.Value = unit.LanguageDLLCreation;
				_dllHelpField.ProjectFile = _projectFile;
				_dllHelpField.Value = unit.LanguageDLLHelp - 79000;
				_dllHotkeyField.ProjectFile = _projectFile;
				_dllHotkeyField.Value = unit.HotKey;

				// Grafik-Werte einfügen
				_graStanding1Field.ElementList = tmpGrList;
				_graStanding1Field.Value = (unit.StandingGraphic1 < 0 ? emptyG : graphicList.FirstOrDefault(g => g.ID == unit.StandingGraphic1));
				_graStanding2Field.ElementList = tmpGrList;
				_graStanding2Field.Value = (unit.StandingGraphic2 < 0 ? emptyG : graphicList.FirstOrDefault(g => g.ID == unit.StandingGraphic2));
				_graFalling1Field.ElementList = tmpGrList;
				_graFalling1Field.Value = (unit.DyingGraphic1 < 0 ? emptyG : graphicList.FirstOrDefault(g => g.ID == unit.DyingGraphic1));
				_graFalling2Field.ElementList = tmpGrList;
				_graFalling2Field.Value = (unit.DyingGraphic2 < 0 ? emptyG : graphicList.FirstOrDefault(g => g.ID == unit.DyingGraphic2));

				// Allgemeine Stats einfügen
				_hitpointsField.Value = unit.HitPoints;
				_losField.Value = (decimal)unit.LineOfSight;
				_speedField.Value = (decimal)unit.Speed;

				// Technische Einstellungen setzen
				_deadModeField.Value = unit.DeathMode > 0;
				_airModeField.Value = unit.AirMode > 0;
				_flyModeField.Value = unit.FlyMode > 0;
				_enabledField.Value = unit.Enabled > 0;
				_unselectableField.Value = unit.Unselectable > 0;
				_edibleMeatField.Value = unit.EdibleMeat > 0;

				// Abmessungen setzen
				_sizeXField.Value = (decimal)unit.SizeRadius1;
				_sizeYField.Value = (decimal)unit.SizeRadius2;
				_editorXField.Value = (decimal)unit.EditorRadius1;
				_editorYField.Value = (decimal)unit.EditorRadius2;
				_selectionXField.Value = (decimal)unit.SelectionRadius1;
				_selectionYField.Value = (decimal)unit.SelectionRadius2;

				// Ressourcen-Speicher setzen
				_resourceStorage1Field.Value = new GenieLibrary.IGenieDataElement.ResourceTuple<int, float, bool>()
				{
					Enabled = unit.ResourceStorages[0].Enabled > 0,
					Type = unit.ResourceStorages[0].Type,
					Amount = unit.ResourceStorages[0].Amount
				};
				_resourceStorage2Field.Value = new GenieLibrary.IGenieDataElement.ResourceTuple<int, float, bool>()
				{
					Enabled = unit.ResourceStorages[1].Enabled > 0,
					Type = unit.ResourceStorages[1].Type,
					Amount = unit.ResourceStorages[1].Amount
				};
				_resourceStorage3Field.Value = new GenieLibrary.IGenieDataElement.ResourceTuple<int, float, bool>()
				{
					Enabled = unit.ResourceStorages[2].Enabled > 0,
					Type = unit.ResourceStorages[2].Type,
					Amount = unit.ResourceStorages[2].Amount
				};

				// Klassenliste in ComboBox einfügen
				_classComboBox.Items.AddRange(Strings.ClassNames.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));

				// Haupt-Werte setzen
				_nameTextBox.Text = unit.Name1.TrimEnd('\0');
				_classComboBox.SelectedIndex = unit.Class;
				_iconIDField.Value = unit.IconID;

				// Sounds setzen
				_soundSelectionField.Value = unit.SelectionSound;
				_soundFallingField.Value = unit.DyingSound;
				_soundTrain1Field.Value = unit.TrainSound1;
				_soundTrain2Field.Value = unit.TrainSound2;

				// Terrains setzen
				_terrPlacement1Field.Value = unit.PlacementTerrain1;
				_terrPlacement2Field.Value = unit.PlacementTerrain2;
				_terrSide1Field.Value = unit.PlacementBypassTerrain1;
				_terrSide2Field.Value = unit.PlacementBypassTerrain2;
				_terrRestrField.Value = unit.TerrainRestriction;

				// Unbekannte Werte setzen
				_unknown1Field.Value = unit.Unknown1;
				_unknown2Field.Value = unit.Unknown2;
				_unknown3Field.Value = (decimal)unit.Unknown3A;
				_unknown4Field.Value = unit.Unknown6;
				_unknown5Field.Value = unit.Unknown8;
				_unknown6Field.Value = unit.UnknownSelectionMode;

				// Steuerungseinstellungen setzen
				_interactionModeField.Value = unit.InteractionMode;
				_minimapModeField.Value = unit.MinimapMode;
				_placementModeField.Value = unit.PlacementMode;
				_attackModeField.Value = unit.AttackMode;
				_hillModeField.Value = unit.HillMode;
				_behaviourModeField.Value = unit.Attribute;
				_interfaceModeField.Value = unit.CommandAttribute;

				// Sonstige Parameter setzen
				_minimapColorField.Value = unit.MinimapColor;
				_civilizationField.Value = unit.Civilization;
				_fogOfWarField.Value = unit.VisibleInFog;
				_blastTypeField.Value = unit.BlastType;
				_garrisonCapacityField.Value = unit.GarrisonCapacity;
				_resourceCapacityField.Value = unit.ResourceCapacity;
				_hpBar1Field.Value = (decimal)unit.HPBarHeight1;
				_hpBar2Field.Value = (decimal)unit.HPBarHeight2;
				_resourceDecayField.Value = (decimal)unit.ResourceDecay;

				// Auswahl-Einstellungen setzen
				_selMaskField.Value = unit.SelectionMask;
				_selShapeTypeField.Value = unit.SelectionShapeType;
				_selShapeField.Value = unit.SelectionShape;
				_selEffectField.Value = unit.SelectionEffect;
				_selEditorColorField.Value = unit.EditorSelectionColor;

				// Schaden-Grafiken setzen
				foreach(var dmg in unit.DamageGraphics)
				{
					// Neue Zeile erstellen
					DataGridViewRow currRow = new DataGridViewRow();
					currRow.Cells.Add(new DataGridViewTextBoxCell() { Value = dmg.DamagePercent });
					currRow.Cells.Add(new DataGridViewTextBoxCell() { Value = dmg.GraphicID });
					currRow.Cells.Add(new DataGridViewTextBoxCell() { Value = dmg.ApplyMode });
					currRow.Cells.Add(new DataGridViewTextBoxCell() { Value = dmg.Unknown2 });
					_dmgGraphicsField.Rows.Add(currRow);
				}
			}

			// Tab-Seiten je nach Typ sperren, sonst jeweilige Steuerelemente füllen
			if(_deadTabPage.Enabled = (unit.Type >= GenieLibrary.DataElements.Civ.Unit.UnitType.DeadFish))
			{
				// Verschiedene Werte setzen
				_graMoving1Field.ElementList = tmpGrList;
				_graMoving1Field.Value = (unit.DeadFish.WalkingGraphic1 < 0 ? emptyG : graphicList.FirstOrDefault(g => g.ID == unit.DeadFish.WalkingGraphic1));
				_graMoving2Field.ElementList = tmpGrList;
				_graMoving2Field.Value = (unit.DeadFish.WalkingGraphic2 < 0 ? emptyG : graphicList.FirstOrDefault(g => g.ID == unit.DeadFish.WalkingGraphic2));
				_rotationSpeedField.Value = (decimal)unit.DeadFish.RotationSpeed;
				_trackUnitDensityField.Value = (decimal)unit.DeadFish.TrackingUnitDensity;

				// Unbekannte Werte setzen
				_unknown7Field.Value = unit.DeadFish.Unknown11;
				_unknown8Field.Value = unit.DeadFish.Unknown16;
				_unknown9AField.Value = unit.DeadFish.Unknown16B[0];
				_unknown9BField.Value = unit.DeadFish.Unknown16B[1];
				_unknown9CField.Value = unit.DeadFish.Unknown16B[2];
				_unknown9DField.Value = unit.DeadFish.Unknown16B[3];
				_unknown9EField.Value = unit.DeadFish.Unknown16B[4];
			}
			if(_birdTabPage.Enabled = (unit.Type >= GenieLibrary.DataElements.Civ.Unit.UnitType.Bird))
			{
				// Verschiedene Werte setzen
				_sheepConvField.Value = unit.Bird.SheepConversion;
				_searchRadiusField.Value = (decimal)unit.Bird.SearchRadius;
				_villagerModeField.Value = unit.Bird.VillagerMode;
				_animalModeField.Value = unit.Bird.AnimalMode;
				_soundAttackField.Value = unit.Bird.AttackSound;
				_soundMoveField.Value = unit.Bird.MoveSound;
				_workRateField.Value = (decimal)unit.Bird.WorkRate;
			}
			if(_type50TabPage.Enabled = (unit.Type >= GenieLibrary.DataElements.Civ.Unit.UnitType.Type50))
			{
				// Rüstungsklassenliste in ComboBox einfügen
				// Diese werden vorher in eine Liste geschrieben, um Indizierung zu ermöglichen, da die DataGridViewCombobox das offenbar nicht unterstützt
				string[] armourClassesArr = Strings.ArmourClasses.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
				List<KeyValuePair<short, string>> armourClasses = new List<KeyValuePair<short, string>>(armourClassesArr.Length);
				for(short i = 0; i < armourClassesArr.Length; ++i)
					armourClasses.Add(new KeyValuePair<short, string>(i, armourClassesArr[i]));
				_attValClassColumn.DataSource = armourClasses;
				_attValClassColumn.DisplayMember = "Value";
				_attValClassColumn.ValueMember = "Key";
				_armValClassColumn.DataSource = armourClasses;
				_armValClassColumn.DisplayMember = "Value";
				_armValClassColumn.ValueMember = "Key";

				// Angriffswerte setzen
				_displayedAttackField.Value = unit.Type50.DisplayedAttack;
				foreach(var attval in unit.Type50.Attacks)
				{
					// Neue Zeile erstellen
					DataGridViewRow currRow = new DataGridViewRow();
					currRow.Cells.Add(new DataGridViewComboBoxCell()
					{
						Value = attval.Key,
						DataSource = armourClasses,
						DisplayMember = "Value",
						ValueMember = "Key"
					});
					currRow.Cells.Add(new DataGridViewTextBoxCell() { Value = attval.Value });
					_attackValuesField.Rows.Add(currRow);
				}

				// Rüstungswerte setzen
				_displayedMeleeArmourField.Value = unit.Type50.DisplayedMeleeArmour;
				_defaultArmorField.Value = unit.Type50.DefaultArmour;
				foreach(var armval in unit.Type50.Armours)
				{
					// Neue Zeile erstellen
					DataGridViewRow currRow = new DataGridViewRow();
					currRow.Cells.Add(new DataGridViewComboBoxCell()
					{
						Value = armval.Key,
						DataSource = armourClasses,
						DisplayMember = "Value",
						ValueMember = "Key"
					});
					currRow.Cells.Add(new DataGridViewTextBoxCell() { Value = armval.Value });
					_armourValuesField.Rows.Add(currRow);
				}

				// Reichweite setzen
				_rangeMinField.Value = (decimal)unit.Type50.MinRange;
				_rangeMaxField.Value = (decimal)unit.Type50.MaxRange;
				_rangeDisplayedField.Value = (decimal)unit.Type50.DisplayedRange;

				// Nachladezeit setzen
				_attackReloadTimeField.Value = (decimal)unit.Type50.ReloadTime;
				_attackReloadTimeDisplayedField.Value = (decimal)unit.Type50.DisplayedReloadTime;

				// Projektilparameter setzen
				_accuracyPercentField.Value = unit.Type50.AccuracyPercent;
				_accuracyErrorField.Value = (decimal)unit.Type50.AccuracyErrorRadius;
				_frameDelayField.Value = unit.Type50.FrameDelay;
				_graphicDisplacementXField.Value = (decimal)unit.Type50.GraphicDisplacement[0];
				_graphicDisplacementYField.Value = (decimal)unit.Type50.GraphicDisplacement[1];
				_graphicDisplacementZField.Value = (decimal)unit.Type50.GraphicDisplacement[2];

				// Verschiedene Werte setzen
				_blastRadiusField.Value = (decimal)unit.Type50.BlastRadius;
				_blastLevelField.Value = unit.Type50.BlastLevel;
				_towerModeField.Value = unit.Type50.TowerMode;
				_terrainMultField.Value = unit.Type50.TerrainRestrictionForDamageMultiplying;
				_graAttackField.ElementList = tmpGrList;
				_graAttackField.Value = (unit.Type50.AttackGraphic < 0 ? emptyG : graphicList.FirstOrDefault(g => g.ID == unit.Type50.AttackGraphic));
			}
			if(_projectileTabPage.Enabled = (unit.Type == GenieLibrary.DataElements.Civ.Unit.UnitType.Projectile))
			{
				// Verschiedene Werte setzen
				_stretchModeField.Value = unit.Projectile.StretchMode;
				_compensationModeField.Value = unit.Projectile.CompensationMode;
				_dropAnimationModeField.Value = unit.Projectile.DropAnimationMode;
				_penetrationModeField.Value = unit.Projectile.PenetrationMode;
				_unknown10Field.Value = unit.Projectile.Unknown24;
				_projectileArcField.Value = (decimal)unit.Projectile.ProjectileArc;
			}
			if(_creatableTabPage.Enabled = (unit.Type >= GenieLibrary.DataElements.Civ.Unit.UnitType.Creatable))
			{
				// Ressourcen-Kosten setzen
				_cost1Field.Value = new GenieLibrary.IGenieDataElement.ResourceTuple<int, float, bool>()
				{
					Enabled = unit.Creatable.ResourceCosts[0].Enabled > 0,
					Type = unit.Creatable.ResourceCosts[0].Type,
					Amount = unit.Creatable.ResourceCosts[0].Amount
				};
				_cost2Field.Value = new GenieLibrary.IGenieDataElement.ResourceTuple<int, float, bool>()
				{
					Enabled = unit.Creatable.ResourceCosts[1].Enabled > 0,
					Type = unit.Creatable.ResourceCosts[1].Type,
					Amount = unit.Creatable.ResourceCosts[1].Amount
				};
				_cost3Field.Value = new GenieLibrary.IGenieDataElement.ResourceTuple<int, float, bool>()
				{
					Enabled = unit.Creatable.ResourceCosts[2].Enabled > 0,
					Type = unit.Creatable.ResourceCosts[2].Type,
					Amount = unit.Creatable.ResourceCosts[2].Amount
				};

				// Geschoss-Werte setzen
				_missileDuplMinField.Value = (decimal)unit.Creatable.DuplicatedMissilesMin;
				_missileDuplMaxField.Value = unit.Creatable.DuplicatedMissilesMax;
				_missileSpawnXField.Value = (decimal)unit.Creatable.MissileSpawningArea[0];
				_missileSpawnYField.Value = (decimal)unit.Creatable.MissileSpawningArea[1];
				_missileSpawnZField.Value = (decimal)unit.Creatable.MissileSpawningArea[2];

				// Verschiedene Werte setzen
				_trainTimeField.Value = unit.Creatable.TrainTime;
				_heroModeField.Value = unit.Creatable.HeroMode;
				_chargeModeField.Value = unit.Creatable.ChargingMode;
				_displayedPierceArmorField.Value = unit.Creatable.DisplayedPierceArmour;

				// Grafiken setzen
				_graGarrisonField.ElementList = tmpGrList;
				_graGarrisonField.Value = (unit.Creatable.GarrisonGraphic < 0 ? emptyG : graphicList.FirstOrDefault(g => g.ID == unit.Creatable.GarrisonGraphic));
				_graChargeField.ElementList = tmpGrList;
				_graChargeField.Value = (unit.Creatable.ChargingGraphic < 0 ? emptyG : graphicList.FirstOrDefault(g => g.ID == unit.Creatable.ChargingGraphic));

				// Unbekannte Werte setzen
				_unknown11Field.Value = unit.Creatable.Unknown26;
				_unknown12Field.Value = unit.Creatable.Unknown27;
				_unknown13Field.Value = unit.Creatable.Unknown28;
			}
			if(_buildingTabPage.Enabled = (unit.Type == GenieLibrary.DataElements.Civ.Unit.UnitType.Building))
			{
				// Grafiken setzen
				_graConstructionField.ElementList = tmpGrList;
				_graConstructionField.Value = (unit.Building.ConstructionGraphicID < 0 ? emptyG : graphicList.FirstOrDefault(g => g.ID == unit.Building.ConstructionGraphicID));
				_graSnowField.ElementList = tmpGrList;
				_graSnowField.Value = (unit.Building.SnowGraphicID < 0 ? emptyG : graphicList.FirstOrDefault(g => g.ID == unit.Building.SnowGraphicID));

				// Verschiedene Werte setzen
				_adjacentModeField.Value = unit.Building.AdjacentMode > 0;
				_disappearsField.Value = unit.Building.DisappearsWhenBuilt > 0;
				_graphicAngleField.Value = unit.Building.GraphicsAngle;
				_foundTerrainField.Value = unit.Building.FoundationTerrainID;
				_constructionSoundField.Value = unit.Building.ConstructionSound;
				_unknownSoundField.Value = unit.Building.UnknownSound;
				_garrisonTypeField.Value = unit.Building.GarrisonType;
				_garrisonHealRateField.Value = (decimal)unit.Building.GarrisonHealRate;

				// Unbekannte Werte setzen
				_unknown14Field.Value = unit.Building.Unknown33;
				_unknown15Field.Value = (decimal)unit.Building.Unknown35;

				// Ablegbare Ressourcen setzen
				_lootWoodField.Value = unit.Building.LootingTable[1] > 0;
				_lootFoodField.Value = unit.Building.LootingTable[4] > 0;
				_lootGoldField.Value = unit.Building.LootingTable[3] > 0;
				_lootStoneField.Value = unit.Building.LootingTable[0] > 0;
			}

			// Fertig
			Application.DoEvents();
			_unitManager.Unlock();
			_formLoaded = true;
		}

		#endregion

		#region Ereignishandler

		#region Fenster

		private void _closeButton_Click(object sender, EventArgs e)
		{
			// Fenster schließen
			this.Close();
		}

		private void EditUnitAttributeForm_ResizeBegin(object sender, EventArgs e)
		{
			// Flackern vermeiden
			this.SuspendLayout();
		}

		private void EditUnitAttributeForm_ResizeEnd(object sender, EventArgs e)
		{
			// Nach Ende des Resize-Vorgangs Zeichnen wieder aktivieren
			this.ResumeLayout();
		}

		#endregion

		#region Tab: Allgemein

		private void _dllNameField_ValueChanged(object sender, Controls.LanguageDLLControl.ValueChangedEventArgs e)
		{
			// ID aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.LanguageDLLName = (ushort)e.NewValue);
			_unitManager.UpdateUnitAttribute(u => u.LanguageDLLHotKeyText = (ushort)e.NewValue + 150000);
		}

		private void _dllDescriptionField_ValueChanged(object sender, Controls.LanguageDLLControl.ValueChangedEventArgs e)
		{
			// ID aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.LanguageDLLCreation = (ushort)e.NewValue);
		}

		private void _dllHelpField_ValueChanged(object sender, Controls.LanguageDLLControl.ValueChangedEventArgs e)
		{
			// ID aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.LanguageDLLHelp = (ushort)e.NewValue + 79000);
		}

		private void _dllHotkeyField_ValueChanged(object sender, Controls.LanguageDLLControl.ValueChangedEventArgs e)
		{
			// ID aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.HotKey = (ushort)e.NewValue);
		}

		private void _hitpointsField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.HitPoints = e.NewValue.SafeConvert<short>());
		}

		private void _losField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.LineOfSight = e.NewValue.SafeConvert<float>());
		}

		private void _speedField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Speed = e.NewValue.SafeConvert<float>());
		}

		private void _deadModeField_ValueChanged(object sender, Controls.CheckBoxFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DeathMode = (byte)(e.NewValue ? 1 : 0));
		}

		private void _airModeField_ValueChanged(object sender, Controls.CheckBoxFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.AirMode = (byte)(e.NewValue ? 1 : 0));
		}

		private void _flyModeField_ValueChanged(object sender, Controls.CheckBoxFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.FlyMode = (byte)(e.NewValue ? 1 : 0));
		}

		private void _enabledField_ValueChanged(object sender, Controls.CheckBoxFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Enabled = (byte)(e.NewValue ? 1 : 0));
		}

		private void _unselectableField_ValueChanged(object sender, Controls.CheckBoxFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Unselectable = (byte)(e.NewValue ? 1 : 0));
		}

		private void _edibleMeatField_ValueChanged(object sender, Controls.CheckBoxFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.EdibleMeat = (byte)(e.NewValue ? 1 : 0));
		}

		private void _sizeXField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.SizeRadius1 = e.NewValue.SafeConvert<float>());
		}

		private void _sizeYField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.SizeRadius2 = e.NewValue.SafeConvert<float>());
		}

		private void _editorXField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.EditorRadius1 = e.NewValue.SafeConvert<float>());
		}

		private void _editorYField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.EditorRadius2 = e.NewValue.SafeConvert<float>());
		}

		private void _selectionXField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.SelectionRadius1 = e.NewValue.SafeConvert<float>());
		}

		private void _selectionYField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.SelectionRadius2 = e.NewValue.SafeConvert<float>());
		}

		private void _resourceStorage1Field_ValueChanged(object sender, Controls.ResourceCostControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.ResourceStorages[0] = new GenieLibrary.IGenieDataElement.ResourceTuple<short, float, byte>()
			{
				Enabled = (byte)(e.NewValue.Enabled ? 1 : 0),
				Type = (short)e.NewValue.Type,
				Amount = e.NewValue.Amount
			});
		}

		private void _resourceStorage2Field_ValueChanged(object sender, Controls.ResourceCostControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.ResourceStorages[1] = new GenieLibrary.IGenieDataElement.ResourceTuple<short, float, byte>()
			{
				Enabled = (byte)(e.NewValue.Enabled ? 1 : 0),
				Type = (short)e.NewValue.Type,
				Amount = e.NewValue.Amount
			});
		}

		private void _resourceStorage3Field_ValueChanged(object sender, Controls.ResourceCostControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.ResourceStorages[2] = new GenieLibrary.IGenieDataElement.ResourceTuple<short, float, byte>()
			{
				Enabled = (byte)(e.NewValue.Enabled ? 1 : 0),
				Type = (short)e.NewValue.Type,
				Amount = e.NewValue.Amount
			});
		}

		private void _nameTextBox_TextChanged(object sender, EventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Name1 = _nameTextBox.Text + "\0");
		}

		private void _classComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Class = (short)_classComboBox.SelectedIndex);
		}

		private void _iconIDField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.IconID = e.NewValue.SafeConvert<short>());
			OnIconChanged(new IconChangedEventArgs(_treeUnit));
		}

		private void _soundSelectionField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.SelectionSound = e.NewValue.SafeConvert<short>());
		}

		private void _soundFallingField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DyingSound = e.NewValue.SafeConvert<short>());
		}

		private void _soundTrain1Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.TrainSound1 = e.NewValue.SafeConvert<short>());
		}

		private void _soundTrain2Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.TrainSound2 = e.NewValue.SafeConvert<short>());
		}

		private void _terrPlacement1Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.PlacementTerrain1 = e.NewValue.SafeConvert<short>());
		}

		private void _terrPlacement2Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.PlacementTerrain2 = e.NewValue.SafeConvert<short>());
		}

		private void _terrSide1Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.PlacementBypassTerrain1 = e.NewValue.SafeConvert<short>());
		}

		private void _terrSide2Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.PlacementBypassTerrain2 = e.NewValue.SafeConvert<short>());
		}

		private void _terrRestrField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.TerrainRestriction = e.NewValue.SafeConvert<short>());
		}

		private void _unknown1Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Unknown1 = e.NewValue.SafeConvert<short>());
		}

		private void _unknown2Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Unknown2 = e.NewValue.SafeConvert<byte>());
		}

		private void _unknown3Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Unknown3A = e.NewValue.SafeConvert<float>());
		}

		private void _unknown4Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Unknown6 = e.NewValue.SafeConvert<byte>());
		}

		private void _unknown5Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Unknown8 = e.NewValue.SafeConvert<byte>());
		}

		private void _unknown6Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.UnknownSelectionMode = e.NewValue.SafeConvert<byte>());
		}

		private void _interactionModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.InteractionMode = e.NewValue.SafeConvert<byte>());
		}

		private void _minimapModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.MinimapMode = e.NewValue.SafeConvert<byte>());
		}

		private void _placementModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.PlacementMode = e.NewValue.SafeConvert<byte>());
		}

		private void _attackModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.AttackMode = e.NewValue.SafeConvert<byte>());
		}

		private void _hillModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.HillMode = e.NewValue.SafeConvert<byte>());
		}

		private void _behaviourModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Attribute = e.NewValue.SafeConvert<byte>());
		}

		private void _interfaceModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.CommandAttribute = e.NewValue.SafeConvert<byte>());
		}

		private void _minimapColorField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.MinimapColor = e.NewValue.SafeConvert<byte>());
		}

		private void _civilizationField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Civilization = e.NewValue.SafeConvert<byte>());
		}

		private void _fogOfWarField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.VisibleInFog = e.NewValue.SafeConvert<byte>());
		}

		private void _blastTypeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.BlastType = e.NewValue.SafeConvert<byte>());
		}

		private void _garrisonCapacityField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.GarrisonCapacity = e.NewValue.SafeConvert<byte>());
		}

		private void _resourceCapacityField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.ResourceCapacity = e.NewValue.SafeConvert<short>());
		}

		private void _hpBar1Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.HPBarHeight1 = e.NewValue.SafeConvert<float>());
		}

		private void _hpBar2Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.HPBarHeight2 = e.NewValue.SafeConvert<float>());
		}

		private void _resourceDecayField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.ResourceDecay = e.NewValue.SafeConvert<float>());
		}

		private void _selMaskField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.SelectionMask = e.NewValue.SafeConvert<byte>());
		}

		private void _selShapeTypeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.SelectionShapeType = e.NewValue.SafeConvert<byte>());
		}

		private void _selShapeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.SelectionShape = e.NewValue.SafeConvert<byte>());
		}

		private void _selEffectField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.SelectionEffect = e.NewValue.SafeConvert<byte>());
		}

		private void _selEditorColorField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.EditorSelectionColor = e.NewValue.SafeConvert<byte>());
		}

		private void _dmgGraphicsField_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			// Formular geladen?
			if(!_formLoaded)
				return;

			// Byte-Spalten überprüfen
			if(e.ColumnIndex == 0 || e.ColumnIndex > 1)
			{
				// Nur Zahlen erlauben
				byte val;
				if(e.RowIndex != _dmgGraphicsField.NewRowIndex && !byte.TryParse((string)e.FormattedValue, out val))
				{
					// Fehlermeldung zeigen
					MessageBox.Show("Fehler: Ungültiger Zellinhalt. Bitte nur Zahlen zwischen 0 und 255 eingeben.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);

					// Bearbeiten erzwingen
					e.Cancel = true;
				}
			}

			// ID-Spalte überprüfen
			if(e.ColumnIndex == 1)
			{
				// Nur Zahlen erlauben
				short val;
				if(e.RowIndex != _dmgGraphicsField.NewRowIndex && (!short.TryParse((string)e.FormattedValue, out val) || val < 0))
				{
					// Fehlermeldung zeigen
					MessageBox.Show("Fehler: Ungültiger Zellinhalt. Bitte nur Zahlen zwischen 0 und 32767 eingeben.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);

					// Bearbeiten erzwingen
					e.Cancel = true;
				}
				return;
			}
		}

		private void _dmgGraphicsField_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			// Formular geladen?
			if(!_formLoaded)
				return;

			// Alle Werte aus View in Objekt schreiben
			_unitManager.UpdateUnitAttribute(u => u.DamageGraphics = new List<GenieLibrary.DataElements.Civ.Unit.DamageGraphic>());
			foreach(DataGridViewRow currRow in _dmgGraphicsField.Rows)
			{
				// Neue Reihe überspringen
				if(currRow.IsNewRow || currRow.Cells[0].Value == null || currRow.Cells[1].Value == null || currRow.Cells[2].Value == null || currRow.Cells[3].Value == null)
					continue;

				// Daten einfügen
				GenieLibrary.DataElements.Civ.Unit.DamageGraphic dmg = new GenieLibrary.DataElements.Civ.Unit.DamageGraphic()
				{
					DamagePercent = (currRow.Cells[0].Value.GetType() == typeof(byte) ? (byte)currRow.Cells[0].Value : byte.Parse((string)currRow.Cells[0].Value)),
					GraphicID = (currRow.Cells[1].Value.GetType() == typeof(short) ? (short)currRow.Cells[1].Value : short.Parse((string)currRow.Cells[1].Value)),
					ApplyMode = (currRow.Cells[2].Value.GetType() == typeof(byte) ? (byte)currRow.Cells[2].Value : byte.Parse((string)currRow.Cells[2].Value)),
					Unknown2 = (currRow.Cells[3].Value.GetType() == typeof(byte) ? (byte)currRow.Cells[3].Value : byte.Parse((string)currRow.Cells[3].Value))
				};
				_unitManager.UpdateUnitAttribute(u => u.DamageGraphics.Add(dmg));
			}
		}

		private void _dmgGraphicsField_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			// Genauso behandelt wie die Zellwert-Änderung
			_dmgGraphicsField_CellValueChanged(sender, null);
		}

		private void _graStanding1Field_ValueChanged(object sender, Controls.DropDownFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.StandingGraphic1 = ((GenieLibrary.DataElements.Graphic)e.NewValue).ID);
		}

		private void _graStanding2Field_ValueChanged(object sender, Controls.DropDownFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.StandingGraphic2 = ((GenieLibrary.DataElements.Graphic)e.NewValue).ID);
		}

		private void _graFalling1Field_ValueChanged(object sender, Controls.DropDownFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DyingGraphic1 = ((GenieLibrary.DataElements.Graphic)e.NewValue).ID);
		}

		private void _graFalling2Field_ValueChanged(object sender, Controls.DropDownFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DyingGraphic2 = ((GenieLibrary.DataElements.Graphic)e.NewValue).ID);
		}

		#endregion

		#region Tab: Tote Einheiten/Fisch

		private void _graMoving1Field_ValueChanged(object sender, Controls.DropDownFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DeadFish.WalkingGraphic1 = ((GenieLibrary.DataElements.Graphic)e.NewValue).ID);
		}

		private void _graMoving2Field_ValueChanged(object sender, Controls.DropDownFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DeadFish.WalkingGraphic2 = ((GenieLibrary.DataElements.Graphic)e.NewValue).ID);
		}

		private void _rotationSpeedField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DeadFish.RotationSpeed = e.NewValue.SafeConvert<float>());
		}

		private void _trackUnitDensityField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DeadFish.TrackingUnitDensity = e.NewValue.SafeConvert<float>());
		}

		private void _unknown7Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DeadFish.Unknown11 = e.NewValue.SafeConvert<byte>());
		}

		private void _unknown8Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DeadFish.Unknown16 = e.NewValue.SafeConvert<byte>());
		}

		private void _unknown9AField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DeadFish.Unknown16B[0] = e.NewValue.SafeConvert<int>());
		}

		private void _unknown9BField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DeadFish.Unknown16B[1] = e.NewValue.SafeConvert<int>());
		}

		private void _unknown9CField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DeadFish.Unknown16B[2] = e.NewValue.SafeConvert<int>());
		}

		private void _unknown9DField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DeadFish.Unknown16B[3] = e.NewValue.SafeConvert<int>());
		}

		private void _unknown9EField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.DeadFish.Unknown16B[4] = e.NewValue.SafeConvert<int>());
		}

		#endregion

		#region Tab: Zivilwerte

		private void _sheepConvField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Bird.SheepConversion = e.NewValue.SafeConvert<short>());
		}

		private void _searchRadiusField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Bird.SearchRadius = e.NewValue.SafeConvert<float>());
		}

		private void _villagerModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Bird.VillagerMode = e.NewValue.SafeConvert<byte>());
		}

		private void _animalModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Bird.AnimalMode = e.NewValue.SafeConvert<byte>());
		}

		private void _soundAttackField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Bird.AttackSound = e.NewValue.SafeConvert<short>());
		}

		private void _soundMoveField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Bird.MoveSound = e.NewValue.SafeConvert<short>());
		}

		private void _workRateField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Bird.WorkRate = e.NewValue.SafeConvert<float>());
		}

		#endregion

		#region Tab: Angriffswerte

		private void _displayedAttackField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.DisplayedAttack = e.NewValue.SafeConvert<short>());
		}

		private void _attackValuesField_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			// Formular geladen?
			if(!_formLoaded)
				return;

			// Wert-Spalte überprüfen
			if(e.ColumnIndex == 1)
			{
				// Nur Zahlen erlauben
				short val;
				if(e.RowIndex != _attackValuesField.NewRowIndex && (!short.TryParse((string)e.FormattedValue, out val) || val < 0))
				{
					// Fehlermeldung zeigen
					MessageBox.Show("Fehler: Ungültiger Zellinhalt. Bitte nur Zahlen zwischen 0 und 32767 eingeben.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);

					// Bearbeiten erzwingen
					e.Cancel = true;
				}
				return;
			}
		}

		private void _attackValuesField_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			// Formular geladen?
			if(!_formLoaded)
				return;

			// Alle Werte aus View in Objekt schreiben
			_unitManager.UpdateUnitAttribute(u => u.Type50.Attacks = new Dictionary<short, short>());
			foreach(DataGridViewRow currRow in _attackValuesField.Rows)
			{
				// Neue Reihe überspringen
				if(currRow.IsNewRow || currRow.Cells[0].Value == null || currRow.Cells[1].Value == null)
					continue;

				// Daten einfügen
				_unitManager.UpdateUnitAttribute(u => u.Type50.Attacks.Add
				(
					(short)currRow.Cells[0].Value,
					currRow.Cells[1].Value.GetType() == typeof(short) ? (short)currRow.Cells[1].Value : short.Parse((string)currRow.Cells[1].Value)
				));
			}
		}

		private void _attackValuesField_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			// Genauso behandeln wie die Zellwert-Änderung
			_attackValuesField_CellValueChanged(sender, null);
		}

		private void _displayedMeleeArmourField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.DisplayedMeleeArmour = e.NewValue.SafeConvert<short>());
		}

		private void _defaultArmorField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.DefaultArmour = e.NewValue.SafeConvert<short>());
		}

		private void _armourValuesField_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			// Formular geladen?
			if(!_formLoaded)
				return;

			// Wert-Spalte überprüfen
			if(e.ColumnIndex == 1)
			{
				// Nur Zahlen erlauben
				short val;
				if(e.RowIndex != _armourValuesField.NewRowIndex && (!short.TryParse((string)e.FormattedValue, out val) || val < 0))
				{
					// Fehlermeldung zeigen
					MessageBox.Show("Fehler: Ungültiger Zellinhalt. Bitte nur Zahlen zwischen 0 und 32767 eingeben.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);

					// Bearbeiten erzwingen
					e.Cancel = true;
				}
				return;
			}
		}

		private void _armourValuesField_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			// Formular geladen?
			if(!_formLoaded)
				return;

			// Alle Werte aus View in Objekt schreiben
			_unitManager.UpdateUnitAttribute(u => u.Type50.Armours = new Dictionary<short, short>());
			foreach(DataGridViewRow currRow in _armourValuesField.Rows)
			{
				// Neue Reihe überspringen
				if(currRow.IsNewRow || currRow.Cells[0].Value == null || currRow.Cells[1].Value == null)
					continue;

				// Daten einfügen
				_unitManager.UpdateUnitAttribute(u => u.Type50.Armours.Add
				(
					(short)currRow.Cells[0].Value,
					currRow.Cells[1].Value.GetType() == typeof(short) ? (short)currRow.Cells[1].Value : short.Parse((string)currRow.Cells[1].Value)
				));
			}
		}

		private void _armourValuesField_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			// Genauso behandeln wie die Zellwert-Änderung
			_armourValuesField_CellValueChanged(sender, null);
		}

		private void _rangeMinField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.MinRange = e.NewValue.SafeConvert<float>());
		}

		private void _rangeMaxField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.MaxRange = e.NewValue.SafeConvert<float>());
		}

		private void _rangeDisplayedField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.DisplayedRange = e.NewValue.SafeConvert<float>());
		}

		private void _attackReloadTimeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.ReloadTime = e.NewValue.SafeConvert<float>());
		}

		private void _attackReloadTimeDisplayedField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.DisplayedReloadTime = e.NewValue.SafeConvert<float>());
		}

		private void _accuracyPercentField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.AccuracyPercent = e.NewValue.SafeConvert<short>());
		}

		private void _accuracyErrorField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.AccuracyErrorRadius = e.NewValue.SafeConvert<float>());
		}

		private void _frameDelayField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.FrameDelay = e.NewValue.SafeConvert<short>());
		}

		private void _graphicDisplacementXField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.GraphicDisplacement[0] = e.NewValue.SafeConvert<float>());
		}

		private void _graphicDisplacementYField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.GraphicDisplacement[1] = e.NewValue.SafeConvert<float>());
		}

		private void _graphicDisplacementZField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.GraphicDisplacement[2] = e.NewValue.SafeConvert<float>());
		}

		private void _blastRadiusField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.BlastRadius = e.NewValue.SafeConvert<float>());
		}

		private void _blastLevelField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.BlastLevel = e.NewValue.SafeConvert<byte>());
		}

		private void _towerModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.TowerMode = e.NewValue.SafeConvert<byte>());
		}

		private void _terrainMultField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.TerrainRestrictionForDamageMultiplying = e.NewValue.SafeConvert<short>());
		}

		private void _graAttackField_ValueChanged(object sender, Controls.DropDownFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Type50.AttackGraphic = ((GenieLibrary.DataElements.Graphic)e.NewValue).ID);
		}

		#endregion

		#region Tab: Projektile

		private void _stretchModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Projectile.StretchMode = e.NewValue.SafeConvert<byte>());
		}

		private void _compensationModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Projectile.CompensationMode = e.NewValue.SafeConvert<byte>());
		}

		private void _dropAnimationModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Projectile.DropAnimationMode = e.NewValue.SafeConvert<byte>());
		}

		private void _penetrationModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Projectile.PenetrationMode = e.NewValue.SafeConvert<byte>());
		}

		private void _unknown10Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Projectile.Unknown24 = e.NewValue.SafeConvert<byte>());
		}

		private void _projectileArcField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Projectile.ProjectileArc = e.NewValue.SafeConvert<float>());
		}

		#endregion

		#region Tab: Erschaffbare Einheiten

		private void _cost1Field_ValueChanged(object sender, Controls.ResourceCostControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.ResourceCosts[0] = new GenieLibrary.IGenieDataElement.ResourceTuple<short, short, short>()
			{
				Enabled = (short)(e.NewValue.Enabled ? 1 : 0),
				Type = (short)e.NewValue.Type,
				Amount = (short)e.NewValue.Amount
			});
		}

		private void _cost2Field_ValueChanged(object sender, Controls.ResourceCostControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.ResourceCosts[1] = new GenieLibrary.IGenieDataElement.ResourceTuple<short, short, short>()
			{
				Enabled = (short)(e.NewValue.Enabled ? 1 : 0),
				Type = (short)e.NewValue.Type,
				Amount = (short)e.NewValue.Amount
			});
		}

		private void _cost3Field_ValueChanged(object sender, Controls.ResourceCostControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.ResourceCosts[2] = new GenieLibrary.IGenieDataElement.ResourceTuple<short, short, short>()
			{
				Enabled = (short)(e.NewValue.Enabled ? 1 : 0),
				Type = (short)e.NewValue.Type,
				Amount = (short)e.NewValue.Amount
			});
		}

		private void _missileDuplMinField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.DuplicatedMissilesMin = e.NewValue.SafeConvert<float>());
		}

		private void _missileDuplMaxField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.DuplicatedMissilesMax = e.NewValue.SafeConvert<byte>());
		}

		private void _missileSpawnXField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.MissileSpawningArea[0] = e.NewValue.SafeConvert<float>());
		}

		private void _missileSpawnYField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.MissileSpawningArea[1] = e.NewValue.SafeConvert<float>());
		}

		private void _missileSpawnZField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.MissileSpawningArea[2] = e.NewValue.SafeConvert<float>());
		}

		private void _trainTimeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.TrainTime = e.NewValue.SafeConvert<short>());
		}

		private void _heroModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.HeroMode = e.NewValue.SafeConvert<byte>());
		}

		private void _chargeModeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.ChargingMode = e.NewValue.SafeConvert<byte>());
		}

		private void _displayedPierceArmorField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.DisplayedPierceArmour = e.NewValue.SafeConvert<short>());
		}

		private void _graGarrisonField_ValueChanged(object sender, Controls.DropDownFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.GarrisonGraphic = ((GenieLibrary.DataElements.Graphic)e.NewValue).ID);
		}

		private void _graChargeField_ValueChanged(object sender, Controls.DropDownFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.ChargingGraphic = ((GenieLibrary.DataElements.Graphic)e.NewValue).ID);
		}

		private void _unknown11Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.Unknown26 = e.NewValue.SafeConvert<int>());
		}

		private void _unknown12Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.Unknown27 = e.NewValue.SafeConvert<int>());
		}

		private void _unknown13Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Creatable.Unknown28 = e.NewValue.SafeConvert<byte>());
		}

		#endregion

		#region Tab: Gebäude

		private void _graConstructionField_ValueChanged(object sender, Controls.DropDownFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.ConstructionGraphicID = ((GenieLibrary.DataElements.Graphic)e.NewValue).ID);
		}

		private void _graSnowField_ValueChanged(object sender, Controls.DropDownFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.SnowGraphicID = ((GenieLibrary.DataElements.Graphic)e.NewValue).ID);
		}

		private void _adjacentModeField_ValueChanged(object sender, Controls.CheckBoxFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.AdjacentMode = (byte)(e.NewValue ? 1 : 0));
		}

		private void _disappearsField_ValueChanged(object sender, Controls.CheckBoxFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.DisappearsWhenBuilt = (byte)(e.NewValue ? 1 : 0));
		}

		private void _graphicAngleField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.GraphicsAngle = e.NewValue.SafeConvert<short>());
		}

		private void _foundTerrainField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.FoundationTerrainID = e.NewValue.SafeConvert<short>());
		}

		private void _constructionSoundField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.ConstructionSound = e.NewValue.SafeConvert<short>());
		}

		private void _unknownSoundField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.UnknownSound = e.NewValue.SafeConvert<short>());
		}

		private void _garrisonTypeField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.GarrisonType = e.NewValue.SafeConvert<byte>());
		}

		private void _garrisonHealRateField_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.GarrisonHealRate = e.NewValue.SafeConvert<float>());
		}

		private void _unknown14Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.Unknown33 = e.NewValue.SafeConvert<byte>());
		}

		private void _unknown15Field_ValueChanged(object sender, Controls.NumberFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.Unknown35 = e.NewValue.SafeConvert<float>());
		}

		private void _lootWoodField_ValueChanged(object sender, Controls.CheckBoxFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.LootingTable[1] = (byte)(e.NewValue ? 1 : 0));
		}

		private void _lootFoodField_ValueChanged(object sender, Controls.CheckBoxFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.LootingTable[4] = (byte)(e.NewValue ? 1 : 0));
		}

		private void _lootGoldField_ValueChanged(object sender, Controls.CheckBoxFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.LootingTable[3] = (byte)(e.NewValue ? 1 : 0));
		}

		private void _lootStoneField_ValueChanged(object sender, Controls.CheckBoxFieldControl.ValueChangedEventArgs e)
		{
			// Wert aktualisieren
			_unitManager.UpdateUnitAttribute(u => u.Building.LootingTable[0] = (byte)(e.NewValue ? 1 : 0));
		}

		#endregion

		#endregion

		#region Ereignisse

		#region Event: Geändertes Icon

		/// <summary>
		/// Wird ausgelöst, wenn sich das Icon der enthaltenen Einheit ändert.
		/// </summary>
		public event IconChangedEventHandler IconChanged;

		/// <summary>
		/// Der Handler-Typ für das IconChanged-Event.
		/// </summary>
		/// <param name="sender">Das auslösende Objekt.</param>
		/// <param name="e">Die Ereignisdaten.</param>
		public delegate void IconChangedEventHandler(object sender, IconChangedEventArgs e);

		/// <summary>
		/// Löst das IconChanged-Ereignis aus.
		/// </summary>
		/// <param name="e">Die Ereignisdaten.</param>
		protected virtual void OnIconChanged(IconChangedEventArgs e)
		{
			IconChangedEventHandler handler = IconChanged;
			if(handler != null)
				handler(this, e);
		}

		/// <summary>
		/// Die Ereignisdaten für das IconChanged-Event.
		/// </summary>
		public class IconChangedEventArgs : EventArgs
		{
			/// <summary>
			/// Die betroffene Einheit.
			/// </summary>
			private TechTreeUnit _unit;

			/// <summary>
			/// Ruft die betroffene Einheit ab.
			/// </summary>
			public TechTreeUnit Unit
			{
				get
				{
					return _unit;
				}
			}

			/// <summary>
			/// Konstruktor.
			/// Erstellt ein neues Ereignisdaten-Objekt mit den gegebenen Daten.
			/// </summary>
			/// <param name="unit">Die betroffene Einheit.</param>
			public IconChangedEventArgs(TechTreeUnit unit)
			{
				// Parameter speichern
				_unit = unit;
			}
		}

		#endregion

		#endregion
	}
}