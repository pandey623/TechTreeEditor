﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TechTreeEditor.TechTreeStructure;

namespace TechTreeEditor
{
	public partial class EditCreatableForm : Form
	{
		#region Variablen

		/// <summary>
		/// Das in diesem Fenster bearbeitbare Gebäude.
		/// </summary>
		private TechTreeCreatable _creatable = null;

		/// <summary>
		/// Die zugrundeliegenden Projektdaten.
		/// </summary>
		private TechTreeFile _projectFile = null;

		#endregion Variablen

		#region Funktionen

		/// <summary>
		/// Konstruktor.
		/// </summary>
		private EditCreatableForm()
		{
			// Steuerelement laden
			InitializeComponent();
		}

		/// <summary>
		/// Erstellt ein neues Einheit-Bearbeitungsfenster.
		/// </summary>
		/// <param name="projectile">Die zu bearbeitende Einheit.</param>
		public EditCreatableForm(TechTreeFile projectFile, TechTreeCreatable unit)
			: this()
		{
			// Parameter merken
			_creatable = unit;
			_projectFile = projectFile;

			// Fenstertitel aktualisieren
			this.Text += _creatable.Name;

			// Dummy-Objekte für leere Listenelemente erstellen
			TechTreeEyeCandy emptyEyeCandy = new TechTreeEyeCandy() { ID = -1 };
			_deadUnitComboBox.Items.Add(emptyEyeCandy);
			_trackUnitComboBox.Items.Add(emptyEyeCandy);
			TechTreeProjectile emptyProj = new TechTreeProjectile() { ID = -1 };
			_projUnitComboBox.Items.Add(emptyProj);
			_projDuplUnitComboBox.Items.Add(emptyProj);

			// Einheitenliste erstellen
			_deadUnitComboBox.DisplayMember = "Name";
			_dropSite1ComboBox.DisplayMember = "Name";
			_dropSite2ComboBox.DisplayMember = "Name";
			List<TechTreeElement> units = _projectFile.Where(e => e is TechTreeUnit);
			_deadUnitComboBox.Items.AddRange(units.ToArray());
			_dropSite1ComboBox.Items.AddRange(units.ToArray());
			_dropSite2ComboBox.Items.AddRange(units.ToArray());

			// Trackingliste erstellen
			_trackUnitComboBox.DisplayMember = "Name";
			_trackUnitComboBox.Items.AddRange(units.Where(u => u.GetType() == typeof(TechTreeEyeCandy)).ToArray());

			// Projektillisten erstellen
			_projUnitComboBox.DisplayMember = "Name";
			_projDuplUnitComboBox.DisplayMember = "Name";
			TechTreeElement[] projectiles = units.Where(p => p.GetType() == typeof(TechTreeProjectile)).ToArray();
			_projUnitComboBox.Items.AddRange(projectiles);
			_projDuplUnitComboBox.Items.AddRange(projectiles);

			// Elemente auswählen
			_deadUnitComboBox.SelectedItem = (_creatable.DeadUnit == null ? emptyEyeCandy : _creatable.DeadUnit);
			_trackUnitComboBox.SelectedItem = (_creatable.TrackingUnit == null ? emptyEyeCandy : _creatable.TrackingUnit);
			_projUnitComboBox.SelectedItem = (_creatable.ProjectileUnit == null ? emptyProj : _creatable.ProjectileUnit);
			_projDuplUnitComboBox.SelectedItem = (_creatable.ProjectileDuplicationUnit == null ? emptyProj : _creatable.ProjectileDuplicationUnit);
			_dropSite1ComboBox.SelectedItem = (_creatable.DropSite1Unit == null ? emptyEyeCandy : _creatable.DropSite1Unit);
			_dropSite2ComboBox.SelectedItem = (_creatable.DropSite2Unit == null ? emptyEyeCandy : _creatable.DropSite2Unit);

			// Kindelemente in View schreiben
			foreach(var currC in _creatable.Children)
			{
				// Zeile erstellen
				DataGridViewRow row = new DataGridViewRow() { Tag = (TechTreeElement)currC.Item2 };
				row.Cells.Add(new DataGridViewTextBoxCell() { Value = currC.Item1.ToString() });
				row.Cells.Add(new DataGridViewTextBoxCell() { Value = currC.Item2.Name });
				_childrenView.Rows.Add(row);
			}
		}

		#endregion Funktionen

		#region Ereignishandler

		private void _deadUnitComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			TechTreeUnit selItem = (TechTreeUnit)_deadUnitComboBox.SelectedItem;
			_creatable.DeadUnit = (selItem.ID >= 0 ? selItem : null);
		}

		private void _projUnitComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Wert aktualisieren
			TechTreeProjectile selItem = (TechTreeProjectile)_projUnitComboBox.SelectedItem;
			_creatable.ProjectileUnit = (selItem.ID >= 0 ? selItem : null);
		}

		private void _projDuplUnitComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Wert aktualisieren
			TechTreeProjectile selItem = (TechTreeProjectile)_projDuplUnitComboBox.SelectedItem;
			_creatable.ProjectileDuplicationUnit = (selItem.ID >= 0 ? selItem : null);
		}

		private void _trackUnitComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Wert aktualisieren
			TechTreeEyeCandy selItem = (TechTreeEyeCandy)_trackUnitComboBox.SelectedItem;
			_creatable.TrackingUnit = (selItem.ID >= 0 ? selItem : null);
		}

		private void _dropSite1ComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			TechTreeUnit selItem = (TechTreeUnit)_dropSite1ComboBox.SelectedItem;
			_creatable.DropSite1Unit = (selItem.ID >= 0 ? selItem : null);
		}

		private void _dropSite2ComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			TechTreeUnit selItem = (TechTreeUnit)_dropSite2ComboBox.SelectedItem;
			_creatable.DropSite2Unit = (selItem.ID >= 0 ? selItem : null);
		}

		private void _childrenView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			// Button-Spalte
			if(e.ColumnIndex == 0)
			{
				// Der Wert muss numerisch sein
				byte val = 0;
				if(!byte.TryParse((string)e.FormattedValue, out val) || val < 0)
				{
					// Fehler
					MessageBox.Show(Strings.Common_Message_PositiveNumber, Strings.Common_Message_PositiveNumber_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
					e.Cancel = true;
				}
				else
				{
					// Wert speichern
					int index = _creatable.Children.FindIndex(c => c.Item2 == (TechTreeElement)_childrenView.Rows[e.RowIndex].Tag);
					_creatable.Children[index] = new Tuple<byte, TechTreeElement>(val, (TechTreeElement)_childrenView.Rows[e.RowIndex].Tag);
				}
			}
		}

		private void _closeButton_Click(object sender, EventArgs e)
		{
			// Fenster schließen
			this.Close();
		}

		#endregion Ereignishandler
	}
}