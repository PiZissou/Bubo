using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Max;
using System.Timers;
using System.Runtime.InteropServices;

namespace Bubo
{
    public partial class BuboUI : UserControl
    {
        bool _isHolding;
        bool _isMouseDown;
        static BuboUI _instance;
        public static BuboUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BuboUI();
                }
                return _instance;
            }
        }
        public BuboUI()
        {
            InitializeComponent();
        }
        public void OnOpen()
        {
            try
            {
                Tools.Format(MethodBase.GetCurrentMethod(), "Begin");
                if (Main.Instance.IsDisposed)
                {
                    Tools.Format(MethodBase.GetCurrentMethod(), "UnDispose");
                    Main.Instance.UnDispose();
                }
                
                Main.Instance.LoadUI();
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }

        }
        public void OnClose()
        {
            try
            {
                Tools.Format(MethodBase.GetCurrentMethod(), "Dispose");

                if (Main.Instance.SelectedTab == 0 && Main.Skin.CurrentSkin is SkinMod mod)
                {
                    bool editEnv = mod.GetEditEnvelopes();
                    mod.SetEditEnvelopesDisplay(false, editEnv);
                    mod.DisplayHoldBones(false);
                }
                else if (Main.Instance.SelectedTab == 2 && Main.Instance.Projection is ProjectDataEngine proj)
                {
                    proj.DisplayVertexColor(false);
                }

                Main.Instance.Dispose();
                if (GetTextUI._defaultDialog != null && GetTextUI._defaultDialog.IsVisible)
                {
                    GetTextUI._defaultDialog.Close();
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void TreeView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is TreeView tv && Main.CurrentEngine is BuboEngine engine && engine.SelectedMaxItem != null)
                {
                    Point pos = tv.PointToScreen(new Point(0d, 0d));
                    if (GetTextUI.OpenGetTextDialog(null, pos, 300, "Rename Channel", "", engine.SelectedMaxItem.Name, GetNameOptions.All) is string newValueS)
                    {
                        if (Main.CurrentEngine.CurrentMod is MorphMod mod)
                        {
                            mod.RenameMaxItem( mod.SelectedItem, newValueS );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private void OnMouseDownTreeView(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is TreeView tv)
                {
                    if (e.ChangedButton == MouseButton.Right)
                    {
                        if (e.ClickCount == 1)
                        {
                            string key = Main.CurrentEngine.KeyTreeViewCM;
                            Tools.Format(MethodBase.GetCurrentMethod(), "Begin");
                            if (tv.FindResource(key) is ContextMenu cm)
                            {
                                tv.ContextMenu = cm;
                            }
                        }
                    }
                    else
                    {
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void OnSelectItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                TreeItem currentIt = null;
                WeightItem currentWIt = null;
                if (!_isHolding && sender is Panel treeview && treeview.DataContext is TreeItem it)
                {
                    currentIt = it;
                }
                else if (!_isHolding && sender is Panel listbox && listbox.DataContext is WeightItem wTem && wTem.LinkedItem is TreeItem it2)
                {
                    currentIt = it2;
                    currentWIt = wTem;
                }
                if (currentIt != null )
                {
                    _isMouseDown = true;

                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        Main.CurrentEngine.SelectItem(currentIt, SelectionItem.SelectToggle , true);
                    }
                    else if (Keyboard.IsKeyDown(Key.LeftShift) && currentWIt == null )
                    {
                        Main.CurrentEngine.SelectItem(currentIt, SelectionItem.SelectMore, true);
                    }
                    else
                    {
                        Main.CurrentEngine.SelectItem(currentIt, SelectionItem.Select, true);
                    }
                    if (Main.CurrentEngine is SkinEngine skEngine && currentWIt != null)
                    {
                        skEngine.CurrentSkin.GetWeightItemSelected();
                    }

                }
                else
                {
                    _isHolding = false;
                }
            }
        }
        private void OnSelectItemMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown && sender is Panel c && c.DataContext is TreeItem it)
            {
                if (!Keyboard.IsKeyDown(Key.LeftShift))
                {
                    Main.CurrentEngine.SelectItem(it, SelectionItem.Select, true);
                }
            }
            else if (_isMouseDown && sender is Panel listbox && listbox.DataContext is WeightItem wItem && wItem.LinkedItem is SkinItem skItem)
            {
                if (!Keyboard.IsKeyDown(Key.LeftShift))
                {
                    Main.CurrentEngine.SelectItem(skItem, SelectionItem.Select, true);
                }
            }
        }
        private void OnSelectItemMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Panel c && c.DataContext is TreeItem)
            {
                _isMouseDown = false;
            }
            else if (sender is Panel listbox )
            {
                _isMouseDown = false;
            }
            //_isMouseDown = false;
        }
        private void OnHoldImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                SkinItem currentIt = null;
                if (sender is Image image && image.DataContext is SkinItem it)
                {
                    currentIt = it;
                }
                else if (sender is Image image2 && image2.DataContext is WeightItem wItem && wItem.LinkedItem is SkinItem it2)
                {
                    currentIt = it2;
                }
                if (currentIt != null)
                {
                    List<TreeItem> sel = new List<TreeItem>(Main.Skin.SelectedItems);
                    bool onOff = !currentIt.IsHold;
                    if (sel.Count > 1 && sel.Exists(x => x.Equals(currentIt)))
                    {
                        _isHolding = true;
                        Main.Skin.CurrentSkin.Hold(sel.Where(x => x is SkinItem).Cast<SkinItem>(), onOff);
                    }
                    else
                    {
                        _isHolding = true;
                        Main.Skin.CurrentSkin.Hold(currentIt, onOff);
                        Main.Skin.SelectItem(currentIt, SelectionItem.Select, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private void OnHoldClick(object sender, RoutedEventArgs e)
        {
            if (Main.Skin.CurrentSkin is SkinMod mod)
            {
                mod.HoldAll(true);
            }
        }
        private void OnUnHoldClick(object sender, RoutedEventArgs e)
        {
            if (Main.Skin.CurrentSkin is SkinMod mod)
            {
                mod.HoldAll(false);
            }
        }
        private void OnAddBoneClick(object sender, RoutedEventArgs e)
        {
            Main.Skin.AddBones(MaxSDK.GetMaxSelection());
        }
        private void OnReplaceBoneClick(object sender, RoutedEventArgs e)
        {
            Main.Skin.ReplaceBones(MaxSDK.GetMaxSelection());
        }
        private void OnResetPoseClick(object sender, RoutedEventArgs e)
        {
            Main.Skin.ResetPos();
        }
        private void OnRemoveUnusedBonesClick(object sender, RoutedEventArgs e)
        {
            if (Main.Skin.CurrentMod is SkinMod mod)
            {
                mod.RemoveUnusedBones();
            }
        }
        private void OnSaveSkinClick(object sender, RoutedEventArgs e)
        {
            Main.Skin.SaveSkin();
        }
        private void OnLoadSkinClick(object sender, RoutedEventArgs e)
        {
            Main.Skin.ReplaceSkin( 1, false);
        }
        private void OnReplaceSkinClick(object sender, RoutedEventArgs e)
        {
            Main.Skin.ReplaceSkin();
        }
        private void OnMirrorGSkinClick(object sender, RoutedEventArgs e)
        {
            Main.Skin.Mirrorkin("G");
        }
        private void OnMirrorDSkinClick(object sender, RoutedEventArgs e)
        {
            Main.Skin.Mirrorkin("D");
        }
        private void OnTransfertWeightBonesClick(object sender, RoutedEventArgs e)
        {
            Main.Skin.TransfertWeightBones();
        }
        private void OnSetAbsoluteWeightClick(object sender, RoutedEventArgs e)
        {
            Main.Skin.SetWeights();
        }
        private void OnSetAbsoluteWeightDQClick(object sender, RoutedEventArgs e)
        {
            Main.Skin.SetDQWeights();
        }
        private void OnWeightPlusClick(object sender, RoutedEventArgs e)
        {
            Main.Skin.WeightsPlus();
        }
        private void OnWeightMinusClick(object sender, RoutedEventArgs e)
        {
            Main.Skin.WeightsMinus();
        }
        private void TabControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TabControl tabs)
            {
                Main.Instance.PreviewTabFirst = true;
            }
        }
        private void RemoveMorphClick(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                MaxSDK.SetCurrentObject(mod.Modifier);
                mod.DeleteChannel(Main.Morph.SelectedIndices);
            }
        }
        private void AddMorphClick(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.AddChannels(MaxSDK.GetMaxSelection());
            }
        }
        private void OnExtractMorphClick(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.Extract(Main.Morph.SelectedIndices);
            }
        }
        private void OnUnusedMorphClick(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.RemoveUnusedChannels(Main.Morph.UnusedTolerance);
            }
        }
        private void OnReplaceMorphClick(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.ReplaceFromSel();
            }
        }
        private void OnOffsetMorphPicked(object sender, PickNodeArgs e)
        {
            if (e.Node is IINode node && Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.Offset(Main.Morph.ProcessingIndices, node);
            }
        }
        private void OnWrappedMorphPicked(object sender, PickNodeArgs e)
        {
            if (e.Node is IINode node && Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.WrapByMorphEngine(Main.Morph.ProcessingIndices, node);
            }
        }
        private void OnCollapseMorphClick(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.Collapse(Main.Morph.ProcessingIndices);
            }
        }
        private void OnDetectSymmetryNodePicked(object sender, PickNodeArgs e)
        {
            if (e.Node is IINode node)
            {
                Main.Instance.DetectSymmetry(node);
            }
        }
        private void OnDetectProjectionNodePicked(object sender, PickNodeArgs e)
        {
            if (e.Node is IINode node)
            {
                Main.Instance.Projection.LoadMapChannels(node);
            }
        }
        private void MorphItemValueChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is SpinnerControl c && c.DataContext is MorphItem item)
            {
                Tools.Format(MethodBase.GetCurrentMethod(), item.Name + " " + item.MaxIndex);
                item.CurrentMod.SetValueChannels( item.Value);
            }
        }
        private void OnActiveChannelCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement c && c.DataContext is MorphItem item)
            {
                item.CurrentMod.SetActiveChannels(!item.IsActive);
            }
        }
        private void MorphItemControlsPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement c && c.DataContext is MorphItem item)
            {
                Main.Morph.ForceProcessInSelection(item);
                _isHolding = true;
            }
        }
        private void OnMirrorDMorphClick(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.MirrorChannels("D", Main.Morph.SelectedIndices);
            }
        }
        private void OnMirrorGMorphClick(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.MirrorChannels("G", Main.Morph.SelectedIndices);
            }
        }
        private void OnMorphHideTargetsClick(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.HideTargets(Main.Morph.SelectedIndices, true);
            }
        }
        private void OnMorphHideAllClick(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.HideTargets(Main.Morph.Indices, true);
            }
        }
        private void OnMorphSelectShowClick(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.HideTargets(Main.Morph.SelectedIndices, false);
                mod.SelectTargets(Main.Morph.SelectedIndices);
            }
        }
        private void OnRenameItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuItem tv && Main.CurrentEngine is BuboEngine engine && engine.SelectedMaxItem != null)
                {
                    Point pos = tv.PointToScreen(new Point(0d, 0d));
                    if (GetTextUI.OpenGetTextDialog(null, pos, 300, "Rename Channel", "", engine.SelectedMaxItem.Name, GetNameOptions.All) is string newValueS)
                    {
                        if (Main.CurrentEngine.CurrentMod is MorphMod mod)
                        {
                            mod.RenameMaxItem(mod.SelectedItem, newValueS);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void OnSkinSelectShowClick(object sender, RoutedEventArgs e)
        {

            if (Main.Skin.CurrentMod is SkinMod mod)
            {
                mod.HideBoneNodes(false);
                mod.SelectBoneNodes();
            }
        }

        private void OnRemoveSkinClick(object sender, RoutedEventArgs e)
        {
            if (Main.Skin.CurrentMod is SkinMod mod)
            {
                mod.RemoveBoneSel();
            }
        }

        private void OnDeleteMorpherClick(object sender, RoutedEventArgs e)
        {
            Main.Morph.DeleteMod();
        }
        private void OnMorphShowConrollerClick(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.ShowChannelController(Main.Morph.SelectedIndices);
            }
        }
        private void OnMorphCurrentToMaxDelta(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.CurrentToMaxDelta(Main.Morph.SelectedIndices , Main.Morph.CurrentPercentToMax );
            }
        }
        private void OnProjectDataClick(object sender, RoutedEventArgs e)
        {
            Main.Instance.Projection.DoProcess();
        }
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is UserControl c)
            {
                if (c.IsVisible)
                {
                    OnOpen();
                }
                else
                {
                    OnClose();
                }
            }
        }
        private void OnInitSkinClick(object sender, RoutedEventArgs e)
        {
            SkinMod.AddSkin(MaxSDK.GetMaxSelection());
        }
        private void OnRemoveZeroWeightsClick(object sender, RoutedEventArgs e)
        {
            if (Main.Skin.CurrentMod is SkinMod mod)
            {
                mod.RemoveZeroWeights(Main.Skin.ZeroWeight);
            }
        }
        private void OnScaleWeightsClick(object sender, RoutedEventArgs e)
        {
            if (Main.Skin.CurrentMod is SkinMod mod)
            {
                mod.SetScaleWeights(Main.Skin.ScaleWeight);
            }
        }
        private void OnSkrinkSkinClick(object sender, RoutedEventArgs e)
        {
            if (Main.Skin.CurrentMod is SkinMod mod)
            {
                mod.Shrink();
            }
        }
        private void OnGrowSkinClick(object sender, RoutedEventArgs e)
        {
            if (Main.Skin.CurrentMod is SkinMod mod)
            {
                mod.Grow();
            }
        }
        private void OnRingSkinClick(object sender, RoutedEventArgs e)
        {
            if (Main.Skin.CurrentMod is SkinMod mod)
            {
                mod.Ring();
            }
        }
        private void OnLoopSkinClick(object sender, RoutedEventArgs e)
        {
            if (Main.Skin.CurrentMod is SkinMod mod)
            {
                mod.Loop();
            }
        }
        private void OnSetWeightClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Button bt)
                {
                    Point pos = bt.PointToScreen(new Point(0d, 0d));
                    if (GetTextUI.OpenGetTextDialog(null, pos, 150 , "Set Weight", "", Main.Skin.AbsoluteWeight.ToString(), GetNameOptions.Numbers) is string newValueS && Double.TryParse(newValueS, out double newValue) && newValue >= 0.0 && newValue <= 1.0)
                    {
                        Main.Skin.AbsoluteWeight = (float)newValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private void OnSetWeightDQClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Button bt)
                {
                    Point pos = bt.PointToScreen(new Point(0d, 0d));

                    if (GetTextUI.OpenGetTextDialog(null, pos, 150 ,"Set Weight", "", Main.Skin.AbsoluteDQWeight.ToString(), GetNameOptions.Numbers) is string newValueS && Double.TryParse(newValueS, out double newValue) && newValue >= 0.0 && newValue <= 1.0)
                    {
                        Main.Skin.AbsoluteDQWeight = (float)newValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private void OnMorphTargetBuilderClick(object sender, RoutedEventArgs e)
        {

            MaxSDK.ExecuteMxs("OpenMorphTargetBuilder()");
        }

        private void OnSettingsOpenConfigClick(object sender, RoutedEventArgs e)
        {
            MaxSDK.ExecuteMxs("Bubo.EditConfig()");
        }
        private void OnInterfaceRefreshClick(object sender, RoutedEventArgs e)
        {
            Main.Instance.Refresh();
        }
        private void OnMorphResetVertexSel(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.ResetVertexSel();
            }
        }

        private void OnMorphSaveXml(object sender, RoutedEventArgs e)
        {
            if (Main.Morph.CurrentMod is MorphMod mod)
            {
                mod.SaveXml();
            }
        }

        private void OnMorphLoadXml(object sender, RoutedEventArgs e)
        {
            if (sender is Control c && Main.Morph.CurrentMod is MorphMod mod && Main.CurrentEngine is MorphEngine engine)
            {
                Point pos = c.PointToScreen(new Point(0d, 0d));
                if (GetTextUI.OpenGetTextDialog(null, pos, 300, "Replace Name", "", "", GetNameOptions.All) is string replaceName)
                {
                   mod.LoadXml(replaceName, engine.MorphIncludeScript);
                }
            }
        }
    }
}

