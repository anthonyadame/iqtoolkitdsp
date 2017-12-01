// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections.Generic;

namespace IQToolkitDSP
{
    using System;
    using System.Collections.Generic;

    /// <summary>Utility class which allows assigning arbitrary annotations to arbitrary objects.</summary>
    /// <typeparam name="T">The type of the objects to annotate.</typeparam>
    internal class Annotations<T>
    {
        /// <summary>Dictionary which stores the annotations. The key is the object to annotate.</summary>
        private Dictionary<T, AnnotationValues> annotations;

        /// <summary>Constructor.</summary>
        public Annotations()
        {
            this.annotations = new Dictionary<T, AnnotationValues>(EqualityComparer<T>.Default);
        }

        /// <summary>Indexer which returns annotations. Just a shortcut syntax for <see cref="GetAnnotation"/>.</summary>
        /// <param name="item">The item to get annotation from.</param>
        /// <param name="annotationName">The name of the annotation to get.</param>
        /// <returns>The annotation if it was found or null otherwise.</returns>
        public object this[T item, string annotationName]
        {
            get { return this.GetAnnotation(item, annotationName); }
        }

        /// <summary>Sets annotation on the specified item.</summary>
        /// <param name="item">The item to annotate.</param>
        /// <param name="annotationName">The name of the annotation to set.</param>
        /// <param name="annotationValue">The value of the annotation to set.</param>
        /// <remarks>If the item already has annotation of the specified name this method will overwrite it.</remarks>
        public void SetAnnotation(T item, string annotationName, object annotationValue)
        {
            AnnotationValues a;
            if (!this.annotations.TryGetValue(item, out a))
            {
                a = new AnnotationValues();
                this.annotations[item] = a;
            }

            a.SetAnnotation(annotationName, annotationValue);
        }

        /// <summary>Determines annotation on the specified item.</summary>
        /// <param name="item">The item to get the annotation from.</param>
        /// <param name="annotationName">The name of the annotation to get.</param>
        /// <returns>The annotation if it was found or null otherwise.</returns>
        public object GetAnnotation(T item, string annotationName)
        {
            AnnotationValues a;
            if (!this.annotations.TryGetValue(item, out a))
            {
                return null;
            }
            else
            {
                return a.GetAnnotation(annotationName);
            }
        }

        /// <summary>Shortcut helper for setting annotations.</summary>
        /// <param name="item">The item to annotate.</param>
        /// <param name="annotationName">The name of the annotation to set.</param>
        /// <param name="annotationValue">The value of the annotation to set.</param>
        /// <returns>Return the annotated item, that is the <paramref name="item"/> unchanged for easier use.</returns>
        public T Annotate(T item, string annotationName, object annotationValue)
        {
            this.SetAnnotation(item, annotationName, annotationValue);
            return item;
        }

        /// <summary>Class to store annotations for a single item.</summary>
        private class AnnotationValues
        {
            /// <summary>Dictionary which stores the annotations. The key is the annotation name and value is the annotation value.</summary>
            private Dictionary<string, object> annotations;

            /// <summary>Constructor</summary>
            public AnnotationValues()
            {
                this.annotations = new Dictionary<string, object>();
            }

            /// <summary>Determines annotation.</summary>
            /// <param name="name">The name of the annotation to get.</param>
            /// <returns>The annotation value or null if no such annotation is present.</returns>
            public object GetAnnotation(string name)
            {
                object value;
                if (!this.annotations.TryGetValue(name, out value))
                {
                    return null;
                }
                else
                {
                    return value;
                }
            }

            /// <summary>Sets annotation.</summary>
            /// <param name="name">The name of the annotation to set.</param>
            /// <param name="value">The value to set the annotation to.</param>
            /// <remarks>If such annotation already exists it will be overwritten.</remarks>
            internal void SetAnnotation(string name, object value)
            {
                this.annotations[name] = value;
            }
        }
    }
}
