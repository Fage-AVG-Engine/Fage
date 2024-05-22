using Fage.Runtime.Collections;
using Fage.Runtime.Layering;
using Fage.Runtime.Layers;
using Fage.Runtime.UI;
using Fage.Runtime.Utility;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Fage.Runtime.Scenes.Main.Branch;

public class BranchOptionPanel
{
	private readonly ContentManager _content;
	private readonly FageTemplateGame _game;
	private readonly MainScene _scene;
	private readonly BranchPanelOptions _panelOptions;
	private readonly ImageButtonTemplate _optionButtonTemplate;

	private ValueList<Rectangle> _optionBBoxes;
	private Texture2D? _preloadedOptionTexture;

	private readonly List<ImageBasedButton> _optionButtons = [];
	private readonly Dictionary<ImageBasedButton, BranchOption> _buttonToOpt = [];

	private readonly InputMaskLayer _inputMask = new("branch options panel input mask", InputMaskMode.AllSources);
	private readonly SealedCompositeLayer _optionDrawableHolder = new("branch options panel");

	public List<BranchOption> Options { get; set; } = [];
	public ILayer EffectiveLayer => _optionDrawableHolder;

	public bool IsActive { get; private set; }

	public Rectangle PanelAvailableArea;
	private bool _alreadyClear;

	public BranchOptionPanel(ContentManager content, FageTemplateGame game, MainScene scene, IOptions<BranchPanelOptions> options)
	{
		_content = content;
		_game = game;
		_scene = scene;
		_panelOptions = options.Value;

		_optionButtonTemplate = new(
			_panelOptions.OptionBackground,
			_panelOptions.OptionPressedBackground,
			_panelOptions.OptionHoverBackground
		)
		{
			Font = _game.GenericFontFamily.GetFont(_panelOptions.HintTextSize),
			ReleasedTextColor = _panelOptions.OptionTextColor,
			HoverTextColor = _panelOptions.OptionHoverTextColor,
			PressedTextColor = _panelOptions.OptionPressedTextColor
		};
	}

	private void OptionSelected(BranchOption branchOption)
	{
		branchOption.SelectedAction(_scene.ScriptingEnvironment);
		Close();
	}

	private void LayoutButtons(List<Point> sizes, int maxButtonWidth)
	{
		Rectangle[] justifiedBBoxes = Layout.VerticalStackJustifyStart(PanelAvailableArea, sizes, 5);

		var buttonsTotalHeight = justifiedBBoxes[^1].Bottom - PanelAvailableArea.Top;
		var buttonsStartY = Layout.AlignCenter(PanelAvailableArea.Y, PanelAvailableArea.Height, buttonsTotalHeight);

		for (int i = 0; i < justifiedBBoxes.Length; i++)
		{
			justifiedBBoxes[i].Y += buttonsStartY; // 移动 bbox，使按钮整体呈垂直居中状态
			Layout.HorizontalAlignCenter(ref justifiedBBoxes[i], PanelAvailableArea.Width); // 水平居中
		}

		for (int i = 0; i < justifiedBBoxes.Length; i++)
		{
			_optionButtons[i].DestinationArea = justifiedBBoxes[i];
		}
	}

	/// <summary>
	/// 手动关闭分支选项面板
	/// </summary>
	public void Close()
	{
		IsActive = false;
		_scene.UnblockScriptExecution();
	}

	public void Show()
	{
		Debug.Assert(_preloadedOptionTexture != null, "调用时机不正确，分支选项按钮资源未加载");

		if (Options.Count == 0)
			throw new ArgumentException($"选项数量不能为0。\n" +
				$"脚本路径{_scene.Script.Path}，\n" +
				$"指令{_scene.Script.CurrentPosition}。");
		_alreadyClear = false;
		_optionDrawableHolder.AddAbove(null, _inputMask);

		int optionNumber = 1;

		int buttonsTotalHeight = 0;
		int maxButtonWidth = 0;

		List<Point> sizes = new(Options.Count);
		foreach (BranchOption option in Options)
		{
			ImageBasedButton optionButton = _optionButtonTemplate.CreateWithText($"{optionNumber}", option.HintText, _content);
			optionButton.LoadResource();
			optionButton.Clicked += button => OptionSelected(_buttonToOpt[button]);

			Point buttonSize = optionButton.ButtonSize;
			buttonsTotalHeight += buttonSize.Y;

			_optionButtons.Add(optionButton);
			_buttonToOpt.Add(optionButton, option);

			_optionDrawableHolder.AddAbove(null, optionButton);

			sizes.Add(buttonSize);

			maxButtonWidth = Math.Max(buttonSize.X, maxButtonWidth);

			optionNumber++;
		}

		LayoutButtons(sizes, maxButtonWidth);

		IsActive = true;
	}

	internal void PreloadResource()
		=> _preloadedOptionTexture = _content.Load<Texture2D>(_panelOptions.OptionBackground);

	internal void UnloadResource()
	{
		_content.UnloadAsset(_panelOptions.OptionBackground);
		_preloadedOptionTexture = null;
	}

	public void CleanupIfInactive()
	{
		if (IsActive || _alreadyClear)
			return;

		Options.Clear();
		_buttonToOpt.Clear();
		_optionBBoxes.Clear();

		foreach (var button in _optionButtons)
		{
			_optionDrawableHolder.Remove(button.Name);
			button.UnloadResource();
		}

		_optionDrawableHolder.Remove(_inputMask.Name);

		_optionButtons.Clear();
		_alreadyClear = true;
	}
}
