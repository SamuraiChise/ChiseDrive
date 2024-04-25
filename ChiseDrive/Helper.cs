using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Cameras;

namespace ChiseDrive
{
    public class Helper
    {
        #region Random Wrapper
        static Random random = new Random();
        public static int Random
        {
            get
            {
                return random.Next();
            }
        }
        public static float Randomf
        {
            get
            {
                return (float)random.NextDouble();
            }
        }
        #endregion

        public static readonly float PI2 = (float)Math.PI * 2.0f;
        /// <summary>
        /// Clamps a float to a radian range of 0 - 2PI
        /// </summary>
        /// <param name="value">Will change the value by reference.</param>
        public static void Radianize(ref float value)
        {
            while (value < 0.0f) value += PI2;
            while (value > PI2) value -= PI2;          
        }

        public static void Clamp(ref Vector3 value, Vector3 min, Vector3 max)
        {
            Clamp(ref value.X, min.X, max.X);
            Clamp(ref value.Y, min.Y, max.Y);
            Clamp(ref value.Z, min.Z, max.Z);
        }

        public static void Clamp(ref Vector3 value, float min, float max)
        {
            Clamp(ref value.X, min, max);
            Clamp(ref value.Y, min, max);
            Clamp(ref value.Z, min, max);
        }

        public static int Clamp(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }

        public static void Clamp(ref int value, int min, int max)
        {
            value = value < min ? min : value > max ? max : value;
        }

        public static void Constrain(ref Vector2 point, ref Rectangle bounds)
        {
            if ((int)point.X < bounds.X) point.X = (float)bounds.X;
            if ((int)point.Y < bounds.Y) point.Y = (float)bounds.Y;
            if ((int)point.X > bounds.X + bounds.Width) point.X = (float)(bounds.X + bounds.Width);
            if ((int)point.Y > bounds.X + bounds.Height) point.X = (float)(bounds.Y + bounds.Height);
        }

        public static void Constrain(ref Vector3 point, ref Rectangle bounds)
        {
            if ((int)point.X < bounds.X) point.X = (float)bounds.X;
            if ((int)point.Y < bounds.Y) point.Y = (float)bounds.Y;
            if ((int)point.X > bounds.X + bounds.Width) point.X = (float)(bounds.X + bounds.Width);
            if ((int)point.Y > bounds.X + bounds.Height) point.X = (float)(bounds.Y + bounds.Height);
        }

        public static void Constrain(ref Rectangle source, ref Rectangle destination, Rectangle constraint)
        {
            int dx = constraint.X - destination.X;
            if (dx < 0) dx = 0;

            int dy = constraint.Y - destination.Y;
            if (dy < 0) dy = 0;

            int dw = (destination.X + destination.Width) - (constraint.X + constraint.Width);
            if (dw < 0) dw = 0;

            int dh = (destination.Y + destination.Height) - (constraint.Y + constraint.Height);
            if (dh < 0) dh = 0;

            // Scale the source by a % of the destination
            int sx = (int)((float)dx * ((float)source.Width / (float)destination.Width));
            int sy = (int)((float)dy * ((float)source.Height / (float)destination.Height));
            int sw = (int)((float)dw * ((float)source.Width / (float)destination.Width));
            int sh = (int)((float)dh * ((float)source.Height / (float)destination.Height));

            // Apply the deltas now
            destination.X += dx;
            destination.Width -= dx;
            destination.Y += dy;
            destination.Height -= dy;
            destination.Width -= dw;
            destination.Height -= dh;

            if (destination.Width < 0) destination.Width = 0;
            if (destination.Height < 0) destination.Height = 0;

            source.X += sx;
            source.Width -= sx;
            source.Y += sy;
            source.Height -= sy;

            source.Width -= sw;
            source.Height -= sh;
        }

        /*
        public static T Clamp<T>(T value, T min, T max)
        {
            value = value < min ? min : value > max ? max : value;
            return value;
        }*/

        public static void Clamp(ref float value, float min, float max)
        {
            if (value < min) value = min;
            if (value > max) value = max;
        }

        public static void Clamp(ref float value, float max)
        {
            if (value > max) value = max;
            if (value < -max) value = -max;
        }

        public static float Theta(Vector2 v1, Vector2 v2)
        {
            float dot = Vector2.Dot(v1, v2);
            float cos = dot / (Helper.Magnitude(v1) * Helper.Magnitude(v2));
            float theta = (float)Math.Acos((double)cos);
            return theta;
        }

        public static float Theta(Vector3 v1, Vector3 v2)
        {
            // This was the math equation I found online, and it seems to work:
            //
            //            -1   V1 . V2
            // Theta = cos    ---------
            //                |V1| |V2|

            float dot = Vector3.Dot(v1, v2);
            float cos = dot / (Helper.Magnitude(v1) * Helper.Magnitude(v2));
            float theta = (float)Math.Acos((double)cos);

            return theta;
        }

        public static float Magnitude(Vector2 vector)
        {
            float magnitute = Vector2.Dot(vector, vector);
            return (float)Math.Sqrt((double)magnitute);
        }

        public static float Magnitude(Vector3 vector)
        {
            //float magnitude = vector.X + vector.Y + vector.Z;
            //magnitude = (float)Math.Sqrt((double)magnitude);
            //return magnitude;

            float magnitude = Vector3.Dot(vector, vector);
            return (float)Math.Sqrt((double)magnitude);
        }

        public static void RandomDisplacement(ref Vector3 position, float scale)
        {
            Vector3 displacement = Vector3.Zero;
            displacement.X = -scale + Randomf * scale * 2f;
            displacement.Y = -scale + Randomf * scale * 2f;
            displacement.Z = -scale + Randomf * scale * 2f;
            position += displacement;
        }

        public static Vector3 RandomDisplacement(Vector3 position, float scale)
        {
            float x = -scale + Randomf * scale * 2.0f;
            float y = -scale + Randomf * scale * 2.0f;
            float z = -scale + Randomf * scale * 2.0f;
            Vector3 newvector = position;
            newvector.X += x;
            newvector.Y += y;
            newvector.Z += z;
            return newvector;
        }

        public static Vector3 RandomDisplacement(Vector3 position, float min, float max)
        {
            float d = max - min;

            float dx = -d + Randomf * d * 2f;
            float dy = -d + Randomf * d * 2f;
            float dz = -d + Randomf * d * 2f;

            Vector3 newvector = position;

            newvector.X = dx < 0f ? dx - min : dx + min;
            newvector.Y = dy < 0f ? dy - min : dy + min;
            newvector.Z = dz < 0f ? dz - min : dz + min;

            return newvector;
        }

        /// <summary>
        /// Clamps a float to a radian range of 0 - 2PI
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <returns>The clamped value.</returns>
        public static float Radianize(float value)
        {
            while (value < 0.0f) value += PI2;
            while (value > PI2) value -= PI2;
            return value; 
        }

        /// <summary>
        /// Clamps an entire vector to radian values.
        /// </summary>
        /// <param name="value">A vector used to store rotations.</param>
        public static void Radianize(ref Vector3 value)
        {
            Radianize(ref value.X);
            Radianize(ref value.Y);
            Radianize(ref value.Z);
        }

        public static Quaternion RotateToFaceQuaternion(Vector3 position, Vector3 target, Vector3 up)
        {
            // Find the matrix that points how we want
            Matrix rotatetoface = Helper.RotateToFace(position, target, up);

            // Add to the rotation, the new matrix
            return Quaternion.CreateFromRotationMatrix(rotatetoface);
        }

        public static Matrix RotateToFace(Vector3 position, Vector3 target, Vector3 up)
        {
            Matrix rotated = Matrix.Identity;

            if (position == target)
            {
                return rotated;
            }

            rotated.Forward = position - target;
            rotated.Forward = Helper.Normalize(rotated.Forward);

            // Floating point error correction/hack
            if (Math.Abs(1f - rotated.Forward.Y) < 0.000001f)
            {
                rotated.Forward = Vector3.Up;
            }

            if (rotated.Forward.Y == up.Y)
            {
                rotated.Right = Vector3.Right;
                rotated.Up = Vector3.Backward;
                return rotated;
            }
            else if (rotated.Forward.Y == -up.Y)
            {
                rotated.Right = Vector3.Left;
                rotated.Up = Vector3.Forward;
                return rotated;
            }
            else if (rotated.Forward == Vector3.Zero)
            {
                // Default incase there's no forward vector?
                rotated.Right = Vector3.Right;
                rotated.Up = Vector3.Up;
                rotated.Forward = Vector3.Forward;
            }

            rotated.Right = Vector3.Cross(rotated.Forward, up);
            rotated.Right = Helper.Normalize(rotated.Right);

            rotated.Up = Vector3.Cross(rotated.Right, rotated.Forward);
            rotated.Up = Helper.Normalize(rotated.Up);

            return rotated;
        }

        public static void Closer(ref float value, float step, Time elapsed)
        {
            if (value > 0f)
            {
                value -= step * elapsed;
                if (value < 0f) value = 0f;
            }
            else
            {
                value += step * elapsed;
                if (value > 0f) value = 0f;
            }
        }

        public static void Closer(ref Vector3 value, float step, Time elapsed)
        {
            Closer(ref value.X, step, elapsed);
            Closer(ref value.Y, step, elapsed);
            Closer(ref value.Z, step, elapsed);            
        }

        public static float Lerp(float initial, float destination, float percent)
        {
            return initial + ((destination - initial) * percent);
        }

        public static Rectangle Lerp(Rectangle initial, Rectangle destination, float percent)
        {
            Rectangle retval = initial;

            Rectangle delta = destination;

            // Find the total delta
            delta.X -= initial.X;
            delta.Y -= initial.Y;
            delta.Width -= initial.Width;
            delta.Height -= initial.Height;

            // Scale by percent
            delta.X = (int)((float)delta.X * percent);
            delta.Y = (int)((float)delta.Y * percent);
            delta.Width = (int)((float)delta.Width * percent);
            delta.Height = (int)((float)delta.Height * percent);

            // Apply the delta
            retval.X += delta.X;
            retval.Y += delta.Y;
            retval.Width += delta.Width;
            retval.Height += delta.Height;

            return retval;
        }

        public static Color EaseTo(Color initial, Color destination, float speed, Time elapsed)
        {
            Color delta = Color.White;

            delta.A = (byte)EaseTo((float)initial.A, (float)destination.A, speed, elapsed);
            delta.R = (byte)EaseTo((float)initial.R, (float)destination.R, speed, elapsed);
            delta.G = (byte)EaseTo((float)initial.G, (float)destination.G, speed, elapsed);
            delta.B = (byte)EaseTo((float)initial.B, (float)destination.B, speed, elapsed);

            return delta;
        }

        public static void EaseTo(ref float initial, float destination, float speed, Time elapsed)
        {
            float delta = destination - initial;

            if (Math.Abs(delta) < speed * elapsed)
            {
                initial = destination;
            }
            else if (delta < 0)
            {
                initial -= (speed * elapsed);
            }
            else
            {
                initial += (speed * elapsed);
            }
        }

        public static float EaseTo(float initial, float destination, float speed, Time elapsed)
        {
            float delta = destination - initial;

            if (Math.Abs(delta) < speed * elapsed)
            {
                return destination;
            }
            else if (delta < 0)
            {
                return initial - (speed * elapsed);
            }
            else
            {
                return initial + (speed * elapsed);
            }
        }

        public static void EaseTo(ref Vector3 initial, Vector3 destination, float speed, Time elapsed)
        {
            Vector3 retvalue = EaseTo(initial, destination, speed, elapsed);
            initial = retvalue;
        }

        public static float SmoothStep(float initial, float destination, float percent)
        {
            Clamp(ref percent, 0f, 1f);

            // Cubicly adjust the percent
            percent = (percent * percent) * (3f - (2f * percent));

            return (initial + ((destination - initial) * percent));
        }

        public static void Normalize(ref Vector3 vec3)
        {
            TestValidity(vec3);
            vec3.Normalize();
            TestValidity(vec3);
        }

        public static Vector3 Normalize(Vector3 vec3)
        {
            if (vec3.X == 0 && vec3.Y == 0 && vec3.Z == 0) throw new SystemException("Error!");
            vec3.Normalize();
            TestValidity(vec3);
            return vec3;
        }

        public static Vector3 EaseTo(Vector3 origin, Vector3 destination, float speed, Time elapsed)
        {
            Vector3 d = destination - origin;
            if (d.Length() <= speed * elapsed)
            {
                return destination;
            }
            else
            {
                d = Helper.Normalize(d);
                d *= speed * elapsed;
                return origin + d;
            }
        }

        public static void EaseTo(ref Vector2 value, Vector2 destination, float speed, Time elapsed)
        {
            Vector2 delta = destination - value;
            if (delta.Length() <= speed * elapsed)
            {
                value = destination;
            }
            else
            {
                delta.Normalize();
                delta *= speed * elapsed;
                value += delta;
            }
        }

        public static bool IsValid(ref Vector3 test)
        {
            if (float.IsNaN(test.X)) return false;
            if (float.IsNaN(test.Y)) return false;
            if (float.IsNaN(test.Z)) return false;
            return true;
        }

        public static void TestValidity(Vector3 test)
        {
            if (float.IsNaN(test.X)) throw new SystemException("Bad X value.");
            if (float.IsNaN(test.Y)) throw new SystemException("Bad Y value.");
            if (float.IsNaN(test.Z)) throw new SystemException("Bad Z value.");
        }

        public static byte CharToHex(char data)
        {
            if (data == '0') return 0;
            if (data == '1') return 1;
            if (data == '2') return 2;
            if (data == '3') return 3;
            if (data == '4') return 4;
            if (data == '5') return 5;
            if (data == '6') return 6;
            if (data == '7') return 7;
            if (data == '8') return 8;
            if (data == '9') return 9;
            if (data == 'A') return 10;
            if (data == 'B') return 11;
            if (data == 'C') return 12;
            if (data == 'D') return 13;
            if (data == 'E') return 14;
            if (data == 'F') return 15;
            else return 0;
        }

        public static Color HexToColor(String data)
        {
            char[] chardata = data.ToCharArray();

            byte r = (byte)(CharToHex(chardata[0]) * 16 + CharToHex(chardata[1]));
            byte g = (byte)(CharToHex(chardata[2]) * 16 + CharToHex(chardata[3]));
            byte b = (byte)(CharToHex(chardata[4]) * 16 + CharToHex(chardata[5]));

            return new Color(r, g, b);
        }

        static public void LineIntersection(ref Vector2 l1s, ref Vector2 l1f,
            ref Vector2 l2s, ref Vector2 l2f, out Vector2? result)
        {
            float denomenator = ((l2f.Y - l2s.Y) * (l1f.X - l1s.X)) -
                                ((l2f.X - l2s.X) * (l1f.Y - l1s.Y));

            if (denomenator == 0f)
            {
                result = null;
                return;
            }

            float numeratora = ((l2f.X - l2s.X) * (l1s.Y - l2s.Y)) -
                                ((l2f.Y - l2s.Y) * (l1s.X - l1f.X));

            float numeratorb = ((l1f.X - l1s.X) * (l1s.Y - l2s.Y)) -
                                ((l1f.Y - l1f.X) * (l1s.X - l2s.X));

            float ua = numeratora / denomenator;
            float ub = numeratorb / denomenator;

            if (ua >= 0f && ua <= 1f && ub >= 0f && ub <= 1f)
            {
                Vector2 resultValue = Vector2.Zero;
                resultValue.X = l1s.X + ua * (l1f.X - l1s.X);
                resultValue.Y = l1s.Y + ub * (l1f.Y - l1s.Y);
                result = resultValue;
            }
            else
            {
                result = null;
            }
        }

        static public void RayIntersectsTriangle2D(ref Ray ray,
            ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out float? result)
        {
            Vector2 v1 = new Vector2(vertex1.X, vertex1.Y);
            Vector2 v2 = new Vector2(vertex2.X, vertex2.Y);
            Vector2 v3 = new Vector2(vertex3.X, vertex3.Y);
            Vector2 r1 = new Vector2(ray.Position.X, ray.Position.Y);
            Vector2 r2 = new Vector2(ray.Position.X + ray.Direction.X * 2000f,
                ray.Position.Y + ray.Direction.Y * 2000f);

            Vector2? retvalue = Vector2.Zero;

            result = null;

            LineIntersection(ref v1, ref v2, ref r1, ref r2, out retvalue);
            if (retvalue != null)
            {
                result = (r1 - (Vector2)retvalue).Length();
                return;
            }

            LineIntersection(ref v2, ref v3, ref r1, ref r2, out retvalue);
            if (retvalue != null)
            {
                result = (r1 - (Vector2)retvalue).Length();
                return;
            }

            LineIntersection(ref v3, ref v1, ref r1, ref r2, out retvalue);
            if (retvalue != null)
            {
                result = (r1 - (Vector2)retvalue).Length();
                return;
            }
        }

        /// <summary>
        /// Checks whether a ray intersects a triangle. This uses the algorithm
        /// developed by Tomas Moller and Ben Trumbore, which was published in the
        /// Journal of Graphics Tools, volume 2, "Fast, Minimum Storage Ray-Triangle
        /// Intersection".
        /// 
        /// This method is implemented using the pass-by-reference versions of the
        /// XNA math functions. Using these overloads is generally not recommended,
        /// because they make the code less readable than the normal pass-by-value
        /// versions. This method can be called very frequently in a tight inner loop,
        /// however, so in this particular case the performance benefits from passing
        /// everything by reference outweigh the loss of readability.
        /// </summary>
        static public void RayIntersectsTriangle(ref Ray ray,
                                          ref Vector3 vertex1,
                                          ref Vector3 vertex2,
                                          ref Vector3 vertex3, out float? result)
        {
            // Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;

            Vector3.Subtract(ref vertex2, ref vertex1, out edge1);
            Vector3.Subtract(ref vertex3, ref vertex1, out edge2);

            // Compute the determinant.
            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant;
            Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                result = null;
                return;
            }

            float inverseDeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            Vector3.Subtract(ref ray.Position, ref vertex1, out distanceVector);

            float triangleU;
            Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1)
            {
                result = null;
                return;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV;
            Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                result = null;
                return;
            }

            // Compute the distance along the ray to the triangle.
            float rayDistance;
            Vector3.Dot(ref edge2, ref distanceCrossEdge1, out rayDistance);
            rayDistance *= inverseDeterminant;

            // Is the triangle behind the ray origin?
            if (rayDistance < 0)
            {
                result = null;
                return;
            }

            result = rayDistance;
        }

        const float Third = 1f / 3f;

        /// <summary>
        /// Determine if the sphere intersects with the polygon
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="vertex3"></param>
        /// <param name="result"></param>
        public static void SphereIntersectsTriangle(ref BoundingSphere sphere,
                                          ref Vector3 vertex1,
                                          ref Vector3 vertex2,
                                          ref Vector3 vertex3, out float? result)
        {
            Vector3 centerpoint = Vector3.Zero;
            TriangleCenter(ref vertex1, ref vertex2, ref vertex3, out centerpoint);

            float r1 = (centerpoint - vertex1).Length();
            float r2 = (centerpoint - vertex2).Length();
            float r3 = (centerpoint - vertex3).Length();

            float longest = r1 > r2 ? r1 > r3 ? r1 : r3 : r2 > r3 ? r2 : r3;

            Vector3 delta = centerpoint - sphere.Center;

            float proximity = delta.Length();

            if (proximity < sphere.Radius) 
                result = sphere.Radius - proximity;
            else 
                result = null;
        }

        public static void TriangleCenter(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out Vector3 center)
        {
            center = (vertex1 * Third) + (vertex2 * Third) + (vertex3 * Third);
        }
    }

 


    /*public static Matrix RotateToFace(Vector3 position, Vector3 target, Vector3 up)
    {
        Vector3 d = target - position;

        Vector3 r = Vector3.Cross(up, d);
        r.Normalize();

        Vector3 b = Vector3.Cross(r, up);
        b.Normalize();

        Vector3 u = Vector3.Cross(b, r);
            
        return new Matrix(r.X, r.Y, r.Z, 0, u.X, u.Y, u.Z, 0, b.X, b.Y, b.Z, 0, 0, 0, 0, 1);  
    }*/
}
