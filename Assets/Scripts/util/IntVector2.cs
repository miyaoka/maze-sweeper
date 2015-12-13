using UnityEngine;
using System.Collections;

public struct IntVector2
{
  public int x;
  public int y;

  public static IntVector2 Zero
  {
    get { return new IntVector2(0, 0); }
  }

  /// <summary>
  /// Creates a new instance of IntVector2.
  /// </summary>
  /// <param name="x">Initial value for the x component of the vector.</param>
  /// <param name="y">Initial value for the y component of the vector.</param>
  public IntVector2(int x, int y)
  {
    this.x = x;
    this.y = y;
  }

  /// <summary>
  /// Creates a new instance of IntVector2.
  /// </summary>
  /// <param name="value">Value to initialise both components to.</param>
  public IntVector2(int value)
  {
    x = value;
    y = value;
  }

  public override string ToString()
  {
    return "X: " + x + ", Y: " + y;
  }

  #region Operators

  public static IntVector2 operator +(IntVector2 a, IntVector2 b)
  {
    return new IntVector2(a.x + b.x, a.y + b.y);
  }

  public static IntVector2 operator -(IntVector2 a, IntVector2 b)
  {
    return new IntVector2(a.x - b.x, a.y - b.y);
  }

  public static IntVector2 operator *(IntVector2 a, int n)
  {
    return new IntVector2(a.x * n, a.y * n);
  }

  public static IntVector2 operator /(IntVector2 a, int n)
  {
    return new IntVector2(a.x / n, a.y / n);
  }

  public static bool operator ==(IntVector2 a, IntVector2 b)
  {
    return (a.x == b.x) && (a.y == b.y);
  }

  public static bool operator !=(IntVector2 a, IntVector2 b)
  {
    return !(a == b);
  }

  public static bool operator <=(IntVector2 a, IntVector2 b)
  {
    return (a.x <= b.x && a.y <= b.y);
  }

  public static bool operator >=(IntVector2 a, IntVector2 b)
  {
    return (a.x >= b.x && a.y >= b.y);
  }

  public static explicit operator IntVector2(Vector2 v)
  {
    return new IntVector2((int)v.x, (int)v.y);
  }

  public static implicit operator Vector2(IntVector2 v)
  {
    return new Vector2(v.x, v.y);
  }

  public static IntVector2 operator %(IntVector2 a, int n)
  {
    return new IntVector2(a.x % n, a.y % n);
  }

  #endregion

  public override bool Equals(object obj)
  {
    if (obj == null)
      return false;

    if (!(obj is IntVector2))
      return false;

    IntVector2 lValue = (IntVector2) obj;
    return (x == lValue.x && y == lValue.y);
  }

  public override int GetHashCode()
  {
    return x ^ y;
  }
}
