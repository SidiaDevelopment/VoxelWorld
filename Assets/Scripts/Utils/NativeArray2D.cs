/*

MIT License

Copyright (c) 2019 Jeffrey Vella
https://github.com/jeffvella/UnityAStarNavigation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using System.Collections;

namespace Unity.Collections
{
    public unsafe struct NativeArray2D<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        private void* _ptr;
        public NativeArray<T> Internal;
        public readonly int YLength;
        public readonly int XLength;

        public NativeArray2D(int x, int y, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            Internal = new NativeArray<T>(x * y, allocator);
            _ptr = Internal.GetUnsafePtr();
            YLength = y;
            XLength = x;
        }
        
        public ref T this[int i] 
            => ref UnsafeUtilityEx.ArrayElementAsRef<T>(_ptr, i);

        public ref T this[int x, int y] 
            => ref UnsafeUtilityEx.ArrayElementAsRef<T>(_ptr, CalculateIndex(x, y));

        public int CalculateIndex(int x, int y) => (x * YLength) + y;

        public int Length => Internal.Length;

        public void Dispose() => Internal.Dispose();

        public IEnumerator<T> GetEnumerator() => Internal.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

