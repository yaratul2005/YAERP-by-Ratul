using System;
using System.Text;

namespace YAERP.Infrastructure.Hardware;

/// <summary>
/// Hardware listener helper for HID barcode wedge scanners (EAN-13, UPC-A, Code128).
/// Detects rapid sequential character inputs (< 100ms inter-keystroke interval) terminated by Enter.
/// </summary>
public class BarcodeScannerListener
{
    private readonly StringBuilder _buffer = new();
    private DateTime _lastKeyTime = DateTime.MinValue;

    /// <summary>Maximum allowed milliseconds between characters to be considered scanner input rather than manual keyboard entry.</summary>
    public int MaxInterKeyDelayMs { get; set; } = 100;

    /// <summary>Minimum barcode character length threshold.</summary>
    public int MinBarcodeLength { get; set; } = 3;

    /// <summary>Raised when a complete barcode is scanned and terminated with Enter.</summary>
    public event EventHandler<string>? BarcodeScanned;

    /// <summary>
    /// Feed a key char into the barcode listener buffer.
    /// Call this from WPF Window KeyDown / PreviewTextInput handlers.
    /// </summary>
    public void ProcessInputChar(char inputChar)
    {
        var now = DateTime.UtcNow;
        var elapsedMs = (now - _lastKeyTime).TotalMilliseconds;
        _lastKeyTime = now;

        // If time between keystrokes is too long, reset buffer (user manual typing)
        if (elapsedMs > MaxInterKeyDelayMs && _buffer.Length > 0)
        {
            _buffer.Clear();
        }

        if (inputChar == '\r' || inputChar == '\n')
        {
            if (_buffer.Length >= MinBarcodeLength)
            {
                string barcode = _buffer.ToString().Trim();
                _buffer.Clear();
                BarcodeScanned?.Invoke(this, barcode);
            }
            else
            {
                _buffer.Clear();
            }
        }
        else if (!char.IsControl(inputChar))
        {
            _buffer.Append(inputChar);
        }
    }

    /// <summary>Clear current scanner buffer.</summary>
    public void Reset()
    {
        _buffer.Clear();
        _lastKeyTime = DateTime.MinValue;
    }
}
