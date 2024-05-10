using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input.InputListeners;
using System.Diagnostics;

namespace Fage.Runtime.Layering;

public class CompositeLayer(string name) : ILayer,
	ILayeredMouseHandler, ILayeredTouchHandler, ILayeredKeyboardHandler, ILayeredGamepadHandler
{
	private readonly List<ILayer> _subLayers = new(4);

	private readonly List<ILayeredMouseHandler> _checksMouse = new(4);
	private readonly List<ILayeredTouchHandler> _checksTouch = new(4);
	private readonly List<ILayeredGamepadHandler> _checksGamepad = new(4);
	private readonly List<ILayeredKeyboardHandler> _checksKeyboard = new(4);

	public string Name { get; } = EnsureNameValid(name);

	private static string EnsureNameValid(string name)
	{
		if (name.Contains("/\\"))
			throw new ArgumentException("场景的名称中不能包含\"/\"或\"\\\"，这些字符已预留给以后的功能。" +
				$"（传入的名称为\"{name}\"）", nameof(name));

		return name;
	}

	public ILayer? Parent { get; set; }

	public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	{
		for (int reverseIterator = _subLayers.Count - 1; reverseIterator != -1; reverseIterator--)
		{
			_subLayers[reverseIterator].Draw(gameTime, spriteBatch);
		}
	}

	public void Update(GameTime gameTime)
	{
		foreach (ILayer layer in _subLayers)
		{
			layer.Update(gameTime);
		}
	}

	#region 内部数据操作

	private void ClassifyLayerToRear(ILayer layer)
	{
		if (layer is ILayeredMouseHandler mouseHandler)
			_checksMouse.Add(mouseHandler);

		if (layer is ILayeredKeyboardHandler keyboardHandler)
			_checksKeyboard.Add(keyboardHandler);

		if (layer is ILayeredTouchHandler touchHandler)
			_checksTouch.Add(touchHandler);

		if (layer is ILayeredGamepadHandler gamepadHandler)
			_checksGamepad.Add(gamepadHandler);
	}

	private void ClassifyLayerToFront(ILayer layer)
	{
		if (layer is ILayeredMouseHandler mouseHandler)
			_checksMouse.Insert(0, mouseHandler);

		if (layer is ILayeredKeyboardHandler keyboardHandler)
			_checksKeyboard.Insert(0, keyboardHandler);

		if (layer is ILayeredTouchHandler touchHandler)
			_checksTouch.Insert(0, touchHandler);

		if (layer is ILayeredGamepadHandler gamepadHandler)
			_checksGamepad.Insert(0, gamepadHandler);
	}

	private void ReclassifyLayers()
	{
		_checksMouse.Clear();
		_checksKeyboard.Clear();
		_checksTouch.Clear();
		_checksGamepad.Clear();

		foreach (var layer in _subLayers)
		{
			ClassifyLayerToRear(layer);
		}
	}

	#endregion

	#region 图层操作

	// optional get getByPath ~~addTo(str path)~~->add_before/add_after

	private bool IsLayerExisting(string layerName)
	{
		foreach (var layer in _subLayers)
			if (layer.Name == layerName)
				return true;

		return false;
	}
	private void EnsureNotAlreadyAdd(ILayer layer)
	{
		if (IsLayerExisting(layer.Name))
			throw new InvalidOperationException($"{Name}图层内，已经添加了同名的子图层");
	}

	public void AddAbove(string? beforeLayerName, ILayer layer)
	{
		EnsureNameValid(layer.Name);
		EnsureNotAlreadyAdd(layer);

		int targetIndex;
		if (beforeLayerName != null)
		{
			EnsureNameValid(beforeLayerName);
			targetIndex = _subLayers.FindIndex(l => l.Name == beforeLayerName);

			if (targetIndex == -1)
				throw new ArgumentException($"找不到'{beforeLayerName}'图层");
		}
		else
		{
			targetIndex = 0;
		}

		if (targetIndex != 0)
		{
			_subLayers.Insert(targetIndex, layer);
			ReclassifyLayers();
		}
		else
		{
			_subLayers.Insert(0, layer);
			ClassifyLayerToFront(layer);
		}

		layer.Parent = this;
	}

	public void AddBehind(string? afterLayerName, ILayer layer)
	{
		EnsureNameValid(layer.Name);
		EnsureNotAlreadyAdd(layer);

		int targetIndex;

		if (afterLayerName != null)
		{
			EnsureNameValid(afterLayerName);
			targetIndex = _subLayers.FindLastIndex(l => l.Name == afterLayerName);

			if (targetIndex == -1)
				throw new ArgumentException($"找不到'{afterLayerName}'图层");
		}
		else
		{
			targetIndex = _subLayers.Count;
		}

		if (targetIndex != _subLayers.Count)
		{
			_subLayers.Insert(targetIndex + 1, layer);
			ReclassifyLayers();
		}
		else
		{
			_subLayers.Insert(0, layer);
			ClassifyLayerToRear(layer);
		}

		layer.Parent = this;
	}

	public void MoveLayerAbove(string movingLayerName, string targetLayer)
	{
		ILayer movingLayer = _subLayers.Find(l => l.Name == movingLayerName)
			?? throw new ArgumentException($"没有子图层{movingLayerName}");

		if (!_subLayers.Remove(movingLayer))
		{
			Debug.WriteLine($"{this.GetLayerPath()}中找不到待移动的子图层({movingLayerName})，可能是数据竞争失败。" +
				$"组合图层的操作不是线程安全的。");
			return;
		}

		int targetIndex = _subLayers.FindLastIndex(l => l.Name == targetLayer);

		_subLayers.Insert(targetIndex, movingLayer);
		ReclassifyLayers();
	}

	public void MoveLayerBehind(string movingLayerName, string targetLayer)
	{
		ILayer movingLayer = _subLayers.Find(l => l.Name == movingLayerName)
			?? throw new ArgumentException($"没有子图层{movingLayerName}");

		if (!_subLayers.Remove(movingLayer))
		{
			Debug.WriteLine($"{this.GetLayerPath()}中找不到待移动的子图层({movingLayerName})，可能是数据竞争失败。" +
				$"组合图层的操作不是线程安全的。");
			return;
		}

		int targetIndex = _subLayers.FindLastIndex(l => l.Name == targetLayer);

		_subLayers.Insert(targetIndex + 1, movingLayer);
		ReclassifyLayers();
	}

	public void Remove(string layerName)
	{
		foreach (var layer in _subLayers)
		{
			if (layer.Name == layerName)
			{
				_subLayers.Remove(layer);

				if (layer is ILayeredMouseHandler mouseHandler)
					_checksMouse.Remove(mouseHandler);

				if (layer is ILayeredKeyboardHandler keyboardHandler)
					_checksKeyboard.Remove(keyboardHandler);

				if (layer is ILayeredTouchHandler touchHandler)
					_checksTouch.Remove(touchHandler);

				if (layer is ILayeredGamepadHandler gamepadHandler)
					_checksGamepad.Remove(gamepadHandler);

				layer.Parent = null;

				return;
			}
		}
	}
	#endregion

	#region 分发输入事件（根图层）

	private void DispatchInput<TEventHandler, TEvent>(List<TEventHandler> handlers, TEvent e)
		where TEventHandler: ILayeredInputHandler<TEvent>
		where TEvent: LayeredInputEventArgs
	{
		for (int i = 0; i < handlers.Count; i++)
			if (handlers[i].HandleInput(this, e))
				break;
	}

	/// <summary>
	/// 作为没有上级图层的根图层时，分发键盘输入事件。
	/// </summary>
	/// <param name="game"></param>
	public void DispatchMouseAsRoot(Game game)
	{
		LayeredMouseEventArgs mouse = new(game);
		DispatchInput(_checksMouse, mouse);
	}

	/// <summary>
	/// 作为没有上级图层的根图层时，分发鼠标输入事件。
	/// </summary>
	/// <param name="game"></param>
	public void DispatchKeyboardAsRoot(Game game)
	{
		LayeredKeyboardEventArgs keyboard = new(game);
		DispatchInput(_checksKeyboard, keyboard);
	}

	/// <summary>
	/// 作为没有上级图层的根图层时，分发触控输入事件。
	/// </summary>
	/// <param name="game"></param>
	/// 
	public void DispatchTouchAsRoot(Game game, TouchEventArgs mgxTouch)
	{
		LayeredTouchEventArgs touch = new(mgxTouch, game);
		DispatchInput(_checksTouch, touch);
	}

	/// <summary>
	/// 作为没有上级图层的根图层时，分发手柄输入事件。
	/// </summary>
	/// <param name="game"></param>
	public void DispatchGamePadAsRoot(Game game, GamePadEventArgs mgxGamePad)
	{
		LayeredGamepadEventArgs gamePad = new(mgxGamePad, game);
		DispatchInput(_checksGamepad, gamePad);
	}
	#endregion

	#region 分发输入事件（子图层）
	private static bool DispatchInputAsChildren<TEventHandler, TEvent>(List<TEventHandler> handlers, ILayer root, TEvent e)
		where TEventHandler : ILayeredInputHandler<TEvent>
		where TEvent : LayeredInputEventArgs
	{
		for (int i = 0; i < handlers.Count; i++)
		{
			if (handlers[i].HandleInput(root, e))
				return true;
		}

		return false;
	}

	public bool HandleInput(ILayer sender, LayeredMouseEventArgs e)
	{
		return DispatchInputAsChildren(_checksMouse, sender, e);
	}

	public bool HandleInput(ILayer sender, LayeredKeyboardEventArgs e)
	{
		return DispatchInputAsChildren(_checksKeyboard, sender, e);
	}

	public bool HandleInput(ILayer sender, LayeredTouchEventArgs e)
	{
		return DispatchInputAsChildren(_checksTouch, sender, e);
	}

	public bool HandleInput(ILayer sender, LayeredGamepadEventArgs e)
	{
		return DispatchInputAsChildren(_checksGamepad, sender, e);
	}
	#endregion

	public override string ToString()
	{
		return $"Composite Layer [{Name}]";
	}
}
