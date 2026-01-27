// Modified by nnra

/*
 * Created by C.J. Kimberlin
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2019
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * 
 * TERMS OF USE - EASING EQUATIONS
 * Open source under the BSD License.
 * Copyright (c)2001 Robert Penner
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 * Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 *
 * ============= Description =============
 *
 * Below is an example of how to use the easing functions in the file. There is a getting function that will return the function
 * from an enum. This is useful since the enum can be exposed in the editor and then the function queried during Start().
 * 
 * Easings.Type ease = Easings.Type.QuadInOut;
 * Easings.EasingFunc func = GetEasingFunction(ease;
 * 
 * float value = func(0, 10, 0.67f);
 * 
 * Easings.EasingFunc derivativeFunc = GetEasingFunctionDerivative(ease);
 * 
 * float derivativeValue = derivativeFunc(0, 10, 0.67f);
 */

using UnityEngine;

namespace SmoothSceneCamera
{
    public static class Easings
    {
        public enum Type
        {
            Linear = 0,
            SineIn,
            SineOut,
            SineInOut,
            QuadIn,
            QuadOut,
            QuadInOut,
            CubicIn,
            CubicOut,
            CubicInOut,
            QuartIn,
            QuartOut,
            QuartInOut,
            QuintIn,
            QuintOut,
            QuintInOut,
            ExpoIn,
            ExpoOut,
            ExpoInOut,
            CircIn,
            CircOut,
            CircInOut,
            EaseInBack,
            EaseOutBack,
            EaseInOutBack,
            EaseInElastic,
            EaseOutElastic,
            EaseInOutElastic,
            EaseInBounce,
            EaseOutBounce,
            EaseInOutBounce,
            Spring,
        }

        private const float NaturalLOGOf2 = 0.693147181f;

        //
        // Type functions
        //

        public static float Linear(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            return Mathf.Lerp(start, end, value);
        }

        public static float Spring(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value = Mathf.Clamp01(value);
            value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) *
                     Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
            return start + (end - start) * value;
        }

        public static float EaseInQuad(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return end * value * value + start;
        }

        public static float EaseOutQuad(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return -end * value * (value - 2) + start;
        }

        public static float EaseInOutQuad(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value + start;
            value--;
            return -end * 0.5f * (value * (value - 2) - 1) + start;
        }

        public static float EaseInCubic(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return end * value * value * value + start;
        }

        public static float EaseOutCubic(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value--;
            end -= start;
            return end * (value * value * value + 1) + start;
        }

        public static float EaseInOutCubic(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value * value + start;
            value -= 2;
            return end * 0.5f * (value * value * value + 2) + start;
        }

        public static float EaseInQuart(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return end * value * value * value * value + start;
        }

        public static float EaseOutQuart(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value--;
            end -= start;
            return -end * (value * value * value * value - 1) + start;
        }

        public static float EaseInOutQuart(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value * value * value + start;
            value -= 2;
            return -end * 0.5f * (value * value * value * value - 2) + start;
        }

        public static float EaseInQuint(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return end * value * value * value * value * value + start;
        }

        public static float EaseOutQuint(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value--;
            end -= start;
            return end * (value * value * value * value * value + 1) + start;
        }

        public static float EaseInOutQuint(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value * value * value * value + start;
            value -= 2;
            return end * 0.5f * (value * value * value * value * value + 2) + start;
        }

        public static float EaseInSine(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return -end * Mathf.Cos(value * (Mathf.PI * 0.5f)) + end + start;
        }

        public static float EaseOutSine(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return end * Mathf.Sin(value * (Mathf.PI * 0.5f)) + start;
        }

        public static float EaseInOutSine(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return -end * 0.5f * (Mathf.Cos(Mathf.PI * value) - 1) + start;
        }

        public static float EaseInExpo(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return end * Mathf.Pow(2, 10 * (value - 1)) + start;
        }

        public static float EaseOutExpo(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return end * (-Mathf.Pow(2, -10 * value) + 1) + start;
        }

        public static float EaseInOutExpo(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * Mathf.Pow(2, 10 * (value - 1)) + start;
            value--;
            return end * 0.5f * (-Mathf.Pow(2, -10 * value) + 2) + start;
        }

        public static float EaseInCirc(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
        }

        public static float EaseOutCirc(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value--;
            end -= start;
            return end * Mathf.Sqrt(1 - value * value) + start;
        }

        public static float EaseInOutCirc(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value /= .5f;
            end -= start;
            if (value < 1) return -end * 0.5f * (Mathf.Sqrt(1 - value * value) - 1) + start;
            value -= 2;
            return end * 0.5f * (Mathf.Sqrt(1 - value * value) + 1) + start;
        }

        public static float EaseInBounce(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            float d = 1f;
            return end - EaseOutBounce(0, end, d - value) + start;
        }

        public static float EaseOutBounce(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value /= 1f;
            end -= start;
            if (value < (1 / 2.75f))
            {
                return end * (7.5625f * value * value) + start;
            }

            if (value < (2 / 2.75f))
            {
                value -= (1.5f / 2.75f);
                return end * (7.5625f * (value) * value + .75f) + start;
            }

            if (value < (2.5 / 2.75))
            {
                value -= (2.25f / 2.75f);
                return end * (7.5625f * (value) * value + .9375f) + start;
            }

            value -= (2.625f / 2.75f);
            return end * (7.5625f * (value) * value + .984375f) + start;
        }

        public static float EaseInOutBounce(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            float d = 1f;
            if (value < d * 0.5f) return EaseInBounce(0, end, value * 2) * 0.5f + start;
            return EaseOutBounce(0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
        }

        public static float EaseInBack(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            value /= 1;
            float s = 1.70158f;
            return end * (value) * value * ((s + 1) * value - s) + start;
        }

        public static float EaseOutBack(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            float s = 1.70158f;
            end -= start;
            value = (value) - 1;
            return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
        }

        public static float EaseInOutBack(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            float s = 1.70158f;
            end -= start;
            value /= .5f;
            if ((value) < 1)
            {
                s *= (1.525f);
                return end * 0.5f * (value * value * (((s) + 1) * value - s)) + start;
            }

            value -= 2;
            s *= (1.525f);
            return end * 0.5f * ((value) * value * (((s) + 1) * value + s) + 2) + start;
        }

        public static float EaseInElastic(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s;
            float a = 0;

            if (value == 0) return start;

            if (Mathf.Approximately((value /= d), 1)) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return -(a * Mathf.Pow(2, 10                        * (value -= 1)) *
                     Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
        }

        public static float EaseOutElastic(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s;
            float a = 0;

            if (value == 0) return start;

            if (Mathf.Approximately((value /= d), 1)) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p * 0.25f;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return (a * Mathf.Pow(2, -10                       * value) *
                    Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
        }

        public static float EaseInOutElastic(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s;
            float a = 0;

            if (value == 0) return start;

            if (Mathf.Approximately((value /= d * 0.5f), 2)) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            if (value < 1)
                return -0.5f * (a * Mathf.Pow(2, 10                        * (value -= 1)) *
                                Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
            return a * Mathf.Pow(2, -10                       * (value -= 1)) *
                   Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)            * 0.5f + end + start;
        }

        //
        // These are derived functions that the motor can use to get the speed at a specific time.
        //
        // The easing functions all work with a normalized time (0 to 1) and the returned value here
        // reflects that. Values returned here should be divided by the actual time.
        //

        public static float LinearD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            return end - start;
        }

        public static float EaseInQuadD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            return 2f * (end - start) * value;
        }

        public static float EaseOutQuadD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return -end * value - end * (value - 2);
        }

        public static float EaseInOutQuadD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value /= .5f;
            end -= start;

            if (value < 1)
            {
                return end * value;
            }

            value--;

            return end * (1 - value);
        }

        public static float EaseInCubicD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            return 3f * (end - start) * value * value;
        }

        public static float EaseOutCubicD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value--;
            end -= start;
            return 3f * end * value * value;
        }

        public static float EaseInOutCubicD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value /= .5f;
            end -= start;

            if (value < 1)
            {
                return (3f / 2f) * end * value * value;
            }

            value -= 2;

            return (3f / 2f) * end * value * value;
        }

        public static float EaseInQuartD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            return 4f * (end - start) * value * value * value;
        }

        public static float EaseOutQuartD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value--;
            end -= start;
            return -4f * end * value * value * value;
        }

        public static float EaseInOutQuartD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value /= .5f;
            end -= start;

            if (value < 1)
            {
                return 2f * end * value * value * value;
            }

            value -= 2;

            return -2f * end * value * value * value;
        }

        public static float EaseInQuintD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            return 5f * (end - start) * value * value * value * value;
        }

        public static float EaseOutQuintD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value--;
            end -= start;
            return 5f * end * value * value * value * value;
        }

        public static float EaseInOutQuintD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value /= .5f;
            end -= start;

            if (value < 1)
            {
                return (5f / 2f) * end * value * value * value * value;
            }

            value -= 2;

            return (5f / 2f) * end * value * value * value * value;
        }

        public static float EaseInSineD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            return (end - start) * 0.5f * Mathf.PI * Mathf.Sin(0.5f * Mathf.PI * value);
        }

        public static float EaseOutSineD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return (Mathf.PI * 0.5f) * end * Mathf.Cos(value * (Mathf.PI * 0.5f));
        }

        public static float EaseInOutSineD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return end * 0.5f * Mathf.PI * Mathf.Sin(Mathf.PI * value);
        }

        public static float EaseInExpoD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            return (10f * NaturalLOGOf2 * (end - start) * Mathf.Pow(2f, 10f * (value - 1)));
        }

        public static float EaseOutExpoD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            return 5f * NaturalLOGOf2 * end * Mathf.Pow(2f, 1f - 10f * value);
        }

        public static float EaseInOutExpoD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value /= .5f;
            end -= start;

            if (value < 1)
            {
                return 5f * NaturalLOGOf2 * end * Mathf.Pow(2f, 10f * (value - 1));
            }

            value--;

            return (5f * NaturalLOGOf2 * end) / (Mathf.Pow(2f, 10f * value));
        }

        public static float EaseInCircD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            return ((end - start) * value) / Mathf.Sqrt(1f - value * value);
        }

        public static float EaseOutCircD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value--;
            end -= start;
            return (-end * value) / Mathf.Sqrt(1f - value * value);
        }

        public static float EaseInOutCircD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value /= .5f;
            end -= start;

            if (value < 1)
            {
                return (end * value) / (2f * Mathf.Sqrt(1f - value * value));
            }

            value -= 2;

            return (-end * value) / (2f * Mathf.Sqrt(1f - value * value));
        }

        public static float EaseInBounceD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            float d = 1f;

            return EaseOutBounceD(0, end, d - value);
        }

        public static float EaseOutBounceD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value /= 1f;
            end -= start;

            if (value < (1 / 2.75f))
            {
                return 2f * end * 7.5625f * value;
            }

            if (value < (2 / 2.75f))
            {
                value -= (1.5f / 2.75f);
                return 2f * end * 7.5625f * value;
            }

            if (value < (2.5 / 2.75))
            {
                value -= (2.25f / 2.75f);
                return 2f * end * 7.5625f * value;
            }

            value -= (2.625f / 2.75f);
            return 2f * end * 7.5625f * value;
        }

        public static float EaseInOutBounceD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;
            float d = 1f;

            if (value < d * 0.5f)
            {
                return EaseInBounceD(0, end, value * 2) * 0.5f;
            }

            return EaseOutBounceD(0, end, value * 2 - d) * 0.5f;
        }

        public static float EaseInBackD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            float s = 1.70158f;

            return 3f * (s + 1f) * (end - start) * value * value - 2f * s * (end - start) * value;
        }

        public static float EaseOutBackD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            float s = 1.70158f;
            end -= start;
            value = (value) - 1;

            return end * ((s + 1f) * value * value + 2f * value * ((s + 1f) * value + s));
        }

        public static float EaseInOutBackD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            float s = 1.70158f;
            end -= start;
            value /= .5f;

            if ((value) < 1)
            {
                s *= (1.525f);
                return 0.5f * end * (s + 1) * value * value + end * value * ((s + 1f) * value - s);
            }

            value -= 2;
            s *= (1.525f);
            return 0.5f * end * ((s + 1) * value * value + 2f * value * ((s + 1f) * value + s));
        }

        public static float EaseInElasticD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            return EaseOutElasticD(start, end, 1f - value);
        }

        public static float EaseOutElasticD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s;
            float a = 0;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p * 0.25f;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return (a * Mathf.PI * d * Mathf.Pow(2f, 1f - 10f * value) *
                    Mathf.Cos((2f * Mathf.PI * (d * value - s)) / p)) / p - 5f * NaturalLOGOf2 * a *
                   Mathf.Pow(2f, 1f - 10f * value)                             *
                   Mathf.Sin((2f * Mathf.PI * (d * value - s)) / p);
        }

        public static float EaseInOutElasticD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s;
            float a = 0;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            if (value < 1)
            {
                value -= 1;

                return -5f * NaturalLOGOf2 * a * Mathf.Pow(2f, 10f * value) *
                       Mathf.Sin(2 * Mathf.PI * (d * value - 2f)   / p) -
                       a * Mathf.PI * d * Mathf.Pow(2f, 10f     * value) *
                       Mathf.Cos(2 * Mathf.PI * (d * value - s) / p) / p;
            }

            value -= 1;

            return a * Mathf.PI * d * Mathf.Cos(2f * Mathf.PI * (d * value - s) / p) /
                   (p * Mathf.Pow(2f, 10f          * value)) -
                   5f * NaturalLOGOf2 * a * Mathf.Sin(2f * Mathf.PI * (d * value - s) / p) /
                   (Mathf.Pow(2f, 10f                                                 * value));
        }

        public static float SpringD(float value, float start = 0, float end = 1)
        {
            if (value is 0 or 1) return value;
            
            value = Mathf.Clamp01(value);
            end -= start;

            // Damn... Thanks http://www.derivative-calculator.net/
            // TODO: And it's a little bit wrong
            return end * (6f * (1f - value) / 5f + 1f) * (-2.2f * Mathf.Pow(1f - value, 1.2f) *
                                                          Mathf.Sin(
                                                              Mathf.PI * value *
                                                              (2.5f * value * value * value +
                                                               0.2f)) +
                                                          Mathf.Pow(1f - value, 2.2f) *
                                                          (Mathf.PI *
                                                           (2.5f * value * value * value + 0.2f) +
                                                           7.5f * Mathf.PI * value * value *
                                                           value) *
                                                          Mathf.Cos(
                                                              Mathf.PI * value *
                                                              (2.5f * value * value * value +
                                                               0.2f)) + 1f) -
                   6f * end * (Mathf.Pow(1 - value, 2.2f) *
                               Mathf.Sin(Mathf.PI * value * (2.5f * value * value * value + 0.2f)) +
                               value
                               / 5f);

        }

        public static float Ease(float value, Type type, float start = 0, float end = 1)
            => GetEasingFunction(type)(value, start, end);

        public delegate float Function(float v, float s = 0, float e = 1);

        /// <summary>
        /// Returns the function associated to the easingFunction enum. This value returned should be cached as it allocates memory
        /// to return.
        /// </summary>
        /// <param name="easingFunction">The enum associated with the type function.</param>
        /// <returns>The type function</returns>
        public static Function GetEasingFunction(Type easingFunction) =>
            easingFunction switch
            {
                Type.QuadIn           => EaseInQuad,
                Type.QuadOut          => EaseOutQuad,
                Type.QuadInOut        => EaseInOutQuad,
                Type.CubicIn          => EaseInCubic,
                Type.CubicOut         => EaseOutCubic,
                Type.CubicInOut       => EaseInOutCubic,
                Type.QuartIn          => EaseInQuart,
                Type.QuartOut         => EaseOutQuart,
                Type.QuartInOut       => EaseInOutQuart,
                Type.QuintIn          => EaseInQuint,
                Type.QuintOut         => EaseOutQuint,
                Type.QuintInOut       => EaseInOutQuint,
                Type.SineIn           => EaseInSine,
                Type.SineOut          => EaseOutSine,
                Type.SineInOut        => EaseInOutSine,
                Type.ExpoIn           => EaseInExpo,
                Type.ExpoOut          => EaseOutExpo,
                Type.ExpoInOut        => EaseInOutExpo,
                Type.CircIn           => EaseInCirc,
                Type.CircOut          => EaseOutCirc,
                Type.CircInOut        => EaseInOutCirc,
                Type.Linear           => Linear,
                Type.Spring           => Spring,
                Type.EaseInBounce     => EaseInBounce,
                Type.EaseOutBounce    => EaseOutBounce,
                Type.EaseInOutBounce  => EaseInOutBounce,
                Type.EaseInBack       => EaseInBack,
                Type.EaseOutBack      => EaseOutBack,
                Type.EaseInOutBack    => EaseInOutBack,
                Type.EaseInElastic    => EaseInElastic,
                Type.EaseOutElastic   => EaseOutElastic,
                Type.EaseInOutElastic => EaseInOutElastic,
                _                     => null
            };

        /// <summary>
        /// Gets the derivative function of the appropriate type function. If you use an type function for position then this
        /// function can get you the speed at a given time (normalized).
        /// </summary>
        /// <param name="easingFunction"></param>
        /// <returns>The derivative function</returns>
        public static Function GetEasingFunctionDerivative(Type easingFunction) =>
            easingFunction switch
            {
                Type.QuadIn           => EaseInQuadD,
                Type.QuadOut          => EaseOutQuadD,
                Type.QuadInOut        => EaseInOutQuadD,
                Type.CubicIn          => EaseInCubicD,
                Type.CubicOut         => EaseOutCubicD,
                Type.CubicInOut       => EaseInOutCubicD,
                Type.QuartIn          => EaseInQuartD,
                Type.QuartOut         => EaseOutQuartD,
                Type.QuartInOut       => EaseInOutQuartD,
                Type.QuintIn          => EaseInQuintD,
                Type.QuintOut         => EaseOutQuintD,
                Type.QuintInOut       => EaseInOutQuintD,
                Type.SineIn           => EaseInSineD,
                Type.SineOut          => EaseOutSineD,
                Type.SineInOut        => EaseInOutSineD,
                Type.ExpoIn           => EaseInExpoD,
                Type.ExpoOut          => EaseOutExpoD,
                Type.ExpoInOut        => EaseInOutExpoD,
                Type.CircIn           => EaseInCircD,
                Type.CircOut          => EaseOutCircD,
                Type.CircInOut        => EaseInOutCircD,
                Type.Linear           => LinearD,
                Type.Spring           => SpringD,
                Type.EaseInBounce     => EaseInBounceD,
                Type.EaseOutBounce    => EaseOutBounceD,
                Type.EaseInOutBounce  => EaseInOutBounceD,
                Type.EaseInBack       => EaseInBackD,
                Type.EaseOutBack      => EaseOutBackD,
                Type.EaseInOutBack    => EaseInOutBackD,
                Type.EaseInElastic    => EaseInElasticD,
                Type.EaseOutElastic   => EaseOutElasticD,
                Type.EaseInOutElastic => EaseInOutElasticD,
                _                     => null
            };
    }
}