// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;


namespace Pixpil.Editor
{
    /// <summary>
    /// Represents an operation that supports Undo/Redo.
    /// </summary>
    public interface IUndoableOperation
    {
        /// <summary>
        /// Gets the description of the operation.
        /// </summary>
        /// <value>The description of the operation.</value>
        /// <remarks>
        /// The description is an object that identifies the operation that is performed. The object is 
        /// typically a static or dynamically generated string (such as "Insert 'abc'", "Backspace",
        /// etc.). The object can be listed in the drop-down menu of an Undo or Redo button.
        /// </remarks>
        object Description { get; }


        /// <summary>
        /// Undoes operation.
        /// </summary>
        void Undo();


        /// <summary>
        /// Performs/Redoes operation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Do")]
        void Do();
    }
    
    
    /// <summary>
    /// Groups the last <i>n</i> operation into one operation that can be undone with a single Undo
    /// command.
    /// </summary>
    internal sealed class UndoGroup : IUndoableOperation
    {
        private readonly List<IUndoableOperation> _undoList = new List<IUndoableOperation>();


        /// <summary>
        /// Gets or sets the description of the operation.
        /// </summary>
        /// <value>The description of the operation.</value>
        public object Description { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="UndoGroup"/> class.
        /// </summary>
        /// <param name="undoStack">The stack of undo operations.</param>
        /// <param name="numberOfOperations">The number of operations to combine.</param>
        /// <param name="description">The description of the operation.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="undoStack"/> is <see langword="null"/>.
        /// </exception>
        public UndoGroup(Deque<IUndoableOperation> undoStack, int numberOfOperations, object description)
        {
            if (undoStack == null)
                throw new ArgumentNullException(nameof(undoStack));

            Description = description;

            Debug.Assert(numberOfOperations > 0, "numberOfOperations should be greater than 0.");
            if (numberOfOperations > undoStack.Count)
                numberOfOperations = undoStack.Count;

            for (int i = 0; i < numberOfOperations; ++i)
                _undoList.Add(undoStack.DequeueHead());
        }


        /// <summary>
        /// Undoes the operation.
        /// </summary>
        public void Undo()
        {
            for (var i = 0; i < _undoList.Count; ++i)
                _undoList[i].Undo();
        }


        /// <summary>
        /// Redoes the operation.
        /// </summary>
        public void Do()
        {
            for (var i = _undoList.Count - 1; i >= 0; --i)
                _undoList[i].Do();
        }
    }
    
    
    /// <summary>
    /// Implements an undo/redo buffer.
    /// </summary>
    public class UndoBuffer : INotifyPropertyChanged
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private int _groupDepth;
        private int _numberOfOperationsInGroup;
        #endregion


        //--------------------------------------------------------------
        #region Events
        //--------------------------------------------------------------

        /// <summary>
        /// Occurs after an operation is undone.
        /// </summary>
        public event EventHandler<EventArgs> OperationUndone;


        /// <summary>
        /// Occurs after an operation is redone.
        /// </summary>
        public event EventHandler<EventArgs> OperationRedone;


        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="UndoBuffer"/> accepts changes.
        /// </summary>
        /// <value><see langword="true"/> if this <see cref="UndoBuffer"/> accepts changes; otherwise, 
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// This property can be used to disable the <see cref="UndoBuffer"/> temporarily while undoing
        /// an operation. Any operations added (see <see cref="Add"/>) while this property is 
        /// <see langword="false"/> will be ignored and will not be recorded in the undo buffer.
        /// </remarks>
        /// <seealso cref="Add"/>
        public bool AcceptChanges
        {
            get => _acceptChanges;
            set
            {
                if (_acceptChanges != value)
                {
                    _acceptChanges = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("AcceptChanges"));
                }
            }
        }
        private bool _acceptChanges = true;


        /// <summary>
        /// Gets a value indicating whether there are operations on the undo stack.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if an operation can be undone; otherwise, <see langword="false"/>.
        /// </value>
        public bool CanUndo => InternalUndoStack.Count > 0;


        /// <summary>
        /// Gets a value indicating whether there are operations on the redo stack.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if an operation can be redone; otherwise, <see langword="false"/>.
        /// </value>
        public bool CanRedo => InternalRedoStack.Count > 0;


        /// <summary>
        /// Gets a value indicating whether an undo group is open.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if an undo group open; otherwise, <see langword="false"/>.
        /// </value>
        /// <seealso cref="BeginUndoGroup"/>
        /// <seealso cref="EndUndoGroup"/>
        public bool IsUndoGroupOpen => _groupDepth != 0;


        /// <summary>
        /// Gets or sets the max number of undo steps stored in the undo buffer.
        /// </summary>
        /// <value>
        /// The max number of undo steps stored in the undo buffer. The default value is 
        /// <see cref="Int32.MaxValue"/>.
        /// </value>
        /// <remarks>
        /// Multiple operations grouped in an undo group by using 
        /// <see cref="BeginUndoGroup"/>/<see cref="EndUndoGroup"/> 
        /// count as a single undo step.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is negative.
        /// </exception>
        public int SizeLimit
        {
            get => _sizeLimit;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "UndoBuffer.SizeLimit must not be negative.");

                if (_sizeLimit != value)
                {
                    _sizeLimit = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("SizeLimit"));
                    if (!IsUndoGroupOpen)
                        EnforceSizeLimit();
                }
            }
        }
        private int _sizeLimit = int.MaxValue;


        /// <summary>
        /// Gets or sets the undo stack.
        /// </summary>
        /// <value>The undo stack.</value>
        /// <remarks>
        /// <para>
        /// The undo stack contains the operations in last-in-first-out order: The first element (see 
        /// <see cref="Deque{T}.Head"/>) contains the operation that was added last and that needs to be
        /// undone first.
        /// </para>
        /// <para>
        /// The number of operations stored in the undo stack can be limited by setting the 
        /// <see cref="SizeLimit"/>. If the number of items in the undo stack exceed the limit, the 
        /// oldest operations are removed from the end of the undo stack (<see cref="Deque{T}.Tail"/>).
        /// </para>
        /// </remarks>
        private Deque<IUndoableOperation> InternalUndoStack { get; set; }


        /// <summary>
        /// Gets or sets the redo stack.
        /// </summary>
        /// <value>The redo stack.</value>
        /// <remarks>
        /// <para>
        /// The redo stack contains the operations in last-in-first-out order: The most-recently undone 
        /// operation is added at the beginning of the redo stack (see <see cref="Deque{T}.Head"/>). 
        /// When operation should be redone they need to be processed in the order from 
        /// <see cref="Deque{T}.Head"/> to <see cref="Deque{T}.Tail"/>.
        /// </para>
        /// <para>
        /// The redo stack is cleared every time a new operation is undone.
        /// </para>
        /// <para>
        /// The number of operations stored in the redo stack can be limited by setting the 
        /// <see cref="SizeLimit"/>. If the number of items in the redo stack exceed the limit, the 
        /// oldest operations are removed from the end of the redo stack (<see cref="Deque{T}.Tail"/>).
        /// </para>
        /// </remarks>
        private Deque<IUndoableOperation> InternalRedoStack { get; set; }


        /// <summary>
        /// Gets the undo stack.
        /// </summary>
        /// <value>The undo stack.</value>
        /// <remarks>
        /// <para>
        /// The undo stack contains the operations in last-in-first-out order: The first element
        /// is the operation that was executed last and that needs to be undone first.
        /// </para>
        /// <para>
        /// This is a read-only collection and should only be used to read information about the most 
        /// recently executed operations. The operations on the undo stack should not be manipulated
        /// directly. Call <see cref="Undo"/> or <see cref="Redo"/> to undo or redo operations.
        /// </para>
        /// </remarks>
        public ReadOnlyCollection<IUndoableOperation> UndoStack { get; private set; }


        /// <summary>
        /// Gets the redo stack.
        /// </summary>
        /// <value>The redo stack.</value>
        /// <remarks>
        /// <para>
        /// The redo stack contains the operations in last-in-first-out order: The most-recently undone 
        /// operation is at the begin of the redo stack.
        /// </para>
        /// <para>
        /// The redo stack is cleared every time a new operation is undone.
        /// </para>
        /// <para>
        /// This is a read-only collection and should only be used to read information about the most 
        /// recently undone operations. The operations on the redo stack should not be manipulated
        /// directly. Call <see cref="Undo"/> or <see cref="Redo"/> to undo or redo operations.
        /// </para>
        /// </remarks>
        public ReadOnlyCollection<IUndoableOperation> RedoStack { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="UndoBuffer"/> class.
        /// </summary>
        public UndoBuffer()
        {
            InternalUndoStack = new Deque<IUndoableOperation>();
            InternalRedoStack = new Deque<IUndoableOperation>();
            UndoStack = new ReadOnlyCollection<IUndoableOperation>(InternalUndoStack);
            RedoStack = new ReadOnlyCollection<IUndoableOperation>(InternalRedoStack);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Begins a new undo group.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An undo group is a group of operations that is combined into a single undo operation.
        /// </para>
        /// <para>
        /// Undo groups can be nested: New undo groups can be started within other undo groups. The
        /// number of <see cref="EndUndoGroup"/> calls need to match the number of 
        /// <see cref="BeginUndoGroup"/> calls. All operations between the outer 
        /// <see cref="BeginUndoGroup"/> and <see cref="EndUndoGroup"/> are combined and pushed onto the
        /// undo stack.
        /// </para>
        /// </remarks>
        /// <seealso cref="EndUndoGroup"/>
        /// <seealso cref="IsUndoGroupOpen"/>
        public void BeginUndoGroup()
        {
            if (_groupDepth == 0)
            {
                _numberOfOperationsInGroup = 0;
                OnPropertyChanged(new PropertyChangedEventArgs("IsUndoGroupOpen"));
            }

            _groupDepth++;
        }


        /// <summary>
        /// Ends an undo group and puts the group of operations onto the <see cref="UndoBuffer"/>.
        /// </summary>
        /// <param name="groupDescription">
        /// The description of the undo group. See <see cref="IUndoableOperation.Description"/> for a
        /// more detailed description. (The parameter is ignored if this is a nested undo group.)
        /// </param>
        /// <remarks>
        /// An undo group is a group of operations that is combined into a single
        /// undo operation.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// There are no undo groups. (<see cref="BeginUndoGroup"/> has not been called.)
        /// </exception>
        /// <seealso cref="BeginUndoGroup"/>
        /// <seealso cref="IsUndoGroupOpen"/>
        public void EndUndoGroup(object groupDescription)
        {
            if (_groupDepth == 0)
                throw new InvalidOperationException("There are no open undo groups.");

            _groupDepth--;
            if (_groupDepth == 0 && _numberOfOperationsInGroup > 1)
            {
                Add(new UndoGroup(InternalUndoStack, _numberOfOperationsInGroup, groupDescription));
                OnPropertyChanged(new PropertyChangedEventArgs("IsUndoGroupOpen"));
            }
        }


        /// <summary>
        /// Checks that no undo groups are open.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// An undo group is open.
        /// </exception>
        private void AssertNoUndoGroupOpen()
        {
            if (IsUndoGroupOpen)
            {
                _groupDepth = 0;
                throw new InvalidOperationException("Cannot Undo/Redo while undo group is open.");
            }
        }


        void EnforceSizeLimit()
        {
            AssertNoUndoGroupOpen();
            while (InternalUndoStack.Count > _sizeLimit)
                InternalUndoStack.DequeueTail();

            while (InternalRedoStack.Count > _sizeLimit)
                InternalRedoStack.DequeueTail();
        }


        /// <summary>
        /// Undoes the last operation.
        /// </summary>
        public void Undo()
        {
            AssertNoUndoGroupOpen();
            if (InternalUndoStack.Count > 0)
            {
                IUndoableOperation operation = InternalUndoStack.DequeueHead();
                InternalRedoStack.EnqueueHead(operation);
                operation.Undo();
                OnOperationUndone();

                if (InternalUndoStack.Count == 0)
                    OnPropertyChanged(new PropertyChangedEventArgs("CanUndo"));

                if (InternalRedoStack.Count == 1)
                    OnPropertyChanged(new PropertyChangedEventArgs("CanRedo"));

                EnforceSizeLimit();
            }
        }


        /// <summary>
        /// Redoes the last undone operation.
        /// </summary>
        public void Redo()
        {
            AssertNoUndoGroupOpen();
            if (InternalRedoStack.Count > 0)
            {
                IUndoableOperation operation = InternalRedoStack.DequeueHead();
                InternalUndoStack.EnqueueHead(operation);
                operation.Do();
                OnOperationRedone();

                if (InternalRedoStack.Count == 0)
                    OnPropertyChanged(new PropertyChangedEventArgs("CanRedo"));

                if (InternalUndoStack.Count == 1)
                    OnPropertyChanged(new PropertyChangedEventArgs("CanUndo"));

                EnforceSizeLimit();
            }
        }


        /// <summary>
        /// Adds an operation to the undo buffer.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <remarks>
        /// <para>
        /// This methods needs to be called for every <see cref="IUndoableOperation"/> before or after
        /// it is executed by calling its <see cref="IUndoableOperation.Do"/> method. Note: 
        /// <see cref="Add"/> does not execute the <see cref="IUndoableOperation"/> it will only record
        /// the operation.
        /// </para>
        /// <para>
        /// After the operation is recorded in the undo buffer it can be undone and redone by calling
        /// <see cref="Undo"/> and <see cref="Redo"/>.
        /// </para>
        /// <para>
        /// The recording of operations in the undo buffer can be temporarily disabled by setting
        /// <see cref="AcceptChanges"/> to <see langword="false"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="operation"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="AcceptChanges"/>
        public void Add(IUndoableOperation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (AcceptChanges)
            {
                InternalUndoStack.EnqueueHead(operation);

                if (IsUndoGroupOpen)
                    _numberOfOperationsInGroup++;

                if (InternalUndoStack.Count == 1)
                    OnPropertyChanged(new PropertyChangedEventArgs("CanUndo"));

                ClearRedoStack();

                if (!IsUndoGroupOpen)
                    EnforceSizeLimit();
            }
        }


        /// <summary>
        /// Clears the undo buffer.
        /// </summary>
        public void ClearAll()
        {
            AssertNoUndoGroupOpen();
            ClearRedoStack();
            ClearUndoStack();
            _numberOfOperationsInGroup = 0;
        }


        private void ClearRedoStack()
        {
            if (InternalRedoStack.Count > 0)
            {
                InternalRedoStack.Clear();
                OnPropertyChanged(new PropertyChangedEventArgs("CanRedo"));
            }
        }


        private void ClearUndoStack()
        {
            if (InternalUndoStack.Count > 0)
            {
                InternalUndoStack.Clear();
                OnPropertyChanged(new PropertyChangedEventArgs("CanUndo"));
            }
        }


        /// <summary>
        /// Raises the <see cref="OperationUndone"/> event.
        /// </summary>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OperationUndone"/> 
        /// in a derived class, be sure to call the base class's <see cref="OperationUndone"/> 
        /// method so that registered delegates receive the event.
        /// </remarks>
        protected void OnOperationUndone() => OperationUndone?.Invoke(this, EventArgs.Empty);


        /// <summary>
        /// Raises the <see cref="OperationRedone"/> event.
        /// </summary>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OperationRedone"/> 
        /// in a derived class, be sure to call the base class's <see cref="OperationRedone"/> 
        /// method so that registered delegates receive the event.
        /// </remarks>
        protected void OnOperationRedone() => OperationRedone?.Invoke(this, EventArgs.Empty);


        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="PropertyChangedEventArgs"/> describing the property that has changed.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnPropertyChanged"/> 
        /// in a derived class, be sure to call the base class's <see cref="OnPropertyChanged"/> 
        /// method so that registered delegates receive the event.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="eventArgs"/> is <see langword="null"/>.
        /// </exception>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs == null)
                throw new ArgumentNullException(nameof(eventArgs));

            PropertyChanged?.Invoke(this, eventArgs);
        }
        #endregion
    }
    
    
    /// <summary>
    /// Helper class which can be used with the <see cref="DebuggerTypeProxyAttribute"/>.
    /// </summary>
    /// <typeparam name="T">The type of items.</typeparam>
    internal sealed class CollectionDebugView< T > {

        private readonly ICollection< T > _collection;
    
    
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionDebugView{T}" /> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public CollectionDebugView(ICollection< T > collection) {
            _collection = collection;
        }


        /// <summary>
        /// Gets the items of the collection.
        /// </summary>
        /// <value>The items of the collection.</value>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => _collection.ToArray();
    }
    
    
    /// <summary>
    /// Represents a double-ended queue (deque) of objects. 
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the deque.</typeparam>
    /// <remarks>
    /// <para>
    /// A deque is similar to a queue except that objects can inserted and removed at both ends.
    /// The capacity of a <see cref="Deque{T}"/> is the number of elements the <see cref="Deque{T}"/> 
    /// can hold. As elements are added to a <see cref="Deque{T}"/>, the capacity is automatically 
    /// increased as required by reallocating the internal array. The capacity can be decreased by 
    /// calling <see cref="TrimExcess"/>. 
    /// </para>
    /// <para>
    /// <see cref="Deque{T}"/> accepts <see langword="null"/> as a valid value for reference types and 
    /// allows duplicate elements.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Deque")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix"), DebuggerDisplay("{GetType().Name,nq}(Count = {Count})")]
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    internal class Deque<T> : IList<T>, ICollection
    {
        //--------------------------------------------------------------
        #region Nested Types
        //--------------------------------------------------------------

        /// <summary>
        /// Enumerates the elements of a <see cref="Deque{T}"/>. 
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            private readonly Deque<T> _deque;
            private readonly int _version;
            private int _index;
            private T _current;


            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public T Current => _current;


            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is positioned before the first element of the collection or after the last 
            /// element.
            /// </exception>
            object IEnumerator.Current
            {
                get
                {
                    if (_index < 0)
                    {
                        if (_index == -1)
                            throw new InvalidOperationException("The enumerator is positioned before the first element of the collection.");
                      
                        throw new InvalidOperationException("The enumerator is positioned after the last element of the collection.");
                    }

                    return _current;
                }
            }


            /// <summary>
            /// Initializes a new instance of the <see cref="Deque{T}.Enumerator"/> struct.
            /// </summary>
            /// <param name="deque">The <see cref="Deque{T}"/> to be enumerated.</param>
            internal Enumerator(Deque<T> deque)
            {
                _deque = deque;
                _version = deque._version;
                _index = -1;
                _current = default(T);
            }


            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting 
            /// unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _index = -2;
                _current = default(T);
            }


            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if the enumerator was successfully advanced to the next element; 
            /// <see langword="false"/> if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Deque")]
            public bool MoveNext()
            {
                if (_version != _deque._version)
                    throw new InvalidOperationException("The Deque<T> was modified after the enumerator was created.");

                if (_index == -2)
                    return false;

                _index++;
                if (_index < _deque._size)
                {
                    T[] array = _deque._array;
                    int head = _deque._head;
                    int length = array.Length;
                    _current = array[(head + _index) % length];
                    return true;
                }

                _index = -2;
                _current = default(T);
                return false;
            }


            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the 
            /// <see cref="Deque{T}"/>.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// The <see cref="Deque{T}"/> was modified after the enumerator was created.
            /// </exception>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Deque")]
            public void Reset()
            {
                if (_version != _deque._version)
                    throw new InvalidOperationException("The Deque<T> was modified after the enumerator was created.");

                _index = -1;
                _current = default(T);
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // ReSharper disable StaticFieldInGenericType
        private static readonly T[] EmptyArray = Array.Empty<T>();
        // ReSharper restore StaticFieldInGenericType

        private T[] _array;
        private int _size;
        private int _head;
        private int _tail;
        private int _version;

        private object _syncRoot;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the number of items contained in the <see cref="Deque{T}"/>.
        /// </summary>
        /// <value>The number of items contained in the <see cref="Deque{T}"/>.</value>
        public int Count => _size;


        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
        /// </summary>
        /// <value>
        /// An object that can be used to synchronize access to the <see cref="ICollection"/>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                    Interlocked.CompareExchange(ref _syncRoot, new object(), null);

                return _syncRoot;
            }
        }


        /// <summary>
        /// Gets a value indicating whether access to the <see cref="ICollection"/> is synchronized 
        /// (thread safe).
        /// </summary>
        /// <value>
        /// <see langword="true"/> if access to the <see cref="ICollection"/> is synchronized (thread 
        /// safe); otherwise, <see langword="false"/>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        bool ICollection.IsSynchronized => false;


        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="ICollection{T}"/> is read-only; otherwise, 
        /// <see langword="false"/>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        bool ICollection<T>.IsReadOnly => false;


        /// <summary>
        /// Gets or sets the object at the beginning of the <see cref="Deque{T}"/>.
        /// </summary>
        /// <value>The object at the beginning of the <see cref="Deque{T}"/>.</value>
        /// <exception cref="InvalidOperationException">The <see cref="Deque{T}"/> is empty.</exception>
        /// <remarks>
        /// This property is an O(1) operation.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Deque")]
        public T Head
        {
            get
            {
                if (_size == 0)
                    throw new InvalidOperationException("The Deque<T> is empty.");

                return _array[_head];
            }
            set
            {
                if (_size == 0)
                    throw new InvalidOperationException("The Deque<T> is empty.");

                _array[_head] = value;
                _version++;
            }
        }


        /// <summary>
        /// Gets or sets the object at the end of the <see cref="Deque{T}"/>.
        /// </summary>
        /// <value>The object at the end of the <see cref="Deque{T}"/>.</value>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="Deque{T}"/> is empty.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This property is an O(1) operation, where n is <see cref="Count"/>.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public T Tail
        {
            get
            {
                if (_size == 0)
                    throw new InvalidOperationException("The Deque<T> is empty.");

                int index = _tail == 0 ? _array.Length - 1 : _tail - 1;
                return _array[index];
            }
            set
            {
                if (_size == 0)
                    throw new InvalidOperationException("The Deque<T> is empty.");

                int index = _tail == 0 ? _array.Length - 1 : _tail - 1;
                _array[index] = value;
                _version++;
            }
        }


        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <value>The item at the specified index.</value>
        /// <param name="index">The zero-based index of the item to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or equal to or greater than <see cref="Count"/>.
        /// </exception>
        /// <remarks>
        /// This indexer is an O(1) operation.
        /// </remarks>
        public T this[int index]
        {
            get
            {
                if (index < 0 || _size <= index)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");

                return _array[(_head + index) % _array.Length];
            }
            set
            {
                if (index < 0 || _size <= index)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");

                _array[(_head + index) % _array.Length] = value;
                _version++;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque{T}"/> class that is empty and has the
        /// default initial capacity. 
        /// </summary>
        public Deque()
        {
            _array = EmptyArray;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Deque{T}"/> class that contains elements copied 
        /// from the specified collection and has sufficient capacity to accommodate the number of 
        /// elements copied. 
        /// </summary>
        /// <param name="collection">
        /// The collection whose elements are copied to the new <see cref="Deque{T}"/>. 
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is <see langword="null"/>.
        /// </exception>
        public Deque(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            var col = collection as ICollection<T>;
            int capacity = (col != null) ? col.Count : 4;
            _array = new T[capacity];

            foreach (T item in collection)
                EnqueueTail(item);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Deque{T}"/> class is empty and has the 
        /// specified initial capacity. 
        /// </summary>
        /// <param name="capacity">
        /// The initial number of elements that the <see cref="Deque{T}"/> can contain.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="capacity"/> is negative.
        /// </exception>
        public Deque(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "The initial capacity of a Deque<T> must not be negative.");

            _array = new T[capacity];
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <param name="item">
        /// The object to add to the <see cref="ICollection{T}"/>.
        /// </param>
        /// <remarks>
        /// <para>
        /// If <see cref="Count"/> already equals the capacity, the capacity of the 
        /// <see cref="Deque{T}"/> is increased by automatically reallocating the internal array, and 
        /// the existing elements are copied to the new array before the new element is added. 
        /// </para>
        /// <para>
        /// If <see cref="Count"/> is less than the capacity of the internal array, this method is an 
        /// O(1) operation. If the internal array needs to be reallocated to accommodate the new 
        /// element, this method becomes an O(n) operation, where n is <see cref="Count"/>. 
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void ICollection<T>.Add(T item) => EnqueueTail(item);


        /// <summary>
        /// Removes all items from the <see cref="Deque{T}"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="Count"/> is set to zero, and references to other objects from elements of the 
        /// collection are also released. 
        /// </para>
        /// <para>
        /// The capacity remains unchanged. To reset the capacity of the <see cref="Deque{T}"/>, call 
        /// <see cref="TrimExcess"/>. Trimming an empty <see cref="Deque{T}"/> sets the capacity of the 
        /// <see cref="Deque{T}"/> to the default capacity. 
        /// </para>
        /// <para>
        /// This method is an O(n) operation, where n is <see cref="Count"/>.
        /// </para>
        /// </remarks>
        public void Clear()
        {
            if (_head < _tail)
            {
                Array.Clear(_array, _head, _size);
            }
            else
            {
                Array.Clear(_array, _head, _array.Length - _head);
                Array.Clear(_array, 0, _tail);
            }

            _size = 0;
            _head = 0;
            _tail = 0;
            _version++;
        }


        /// <summary>
        /// Determines whether the <see cref="Deque{T}"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="Deque{T}"/>.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="item"/> is found in the <see cref="Deque{T}"/>; 
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method determines equality using the default equality comparer 
        /// <see cref="EqualityComparer{T}.Default"/> for <typeparamref name="T"/>, the type of values 
        /// in the <see cref="Deque{T}"/>. 
        /// </para>
        /// <para>
        /// This method performs a linear search; therefore, this method is an O(n) operation, where n 
        /// is <see cref="Count"/>. 
        /// </para>
        /// </remarks>
        public bool Contains(T item) => IndexOf(item) != -1;


        /// <summary>
        /// Determines the index of a specific item in the <see cref="Deque{T}"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="Deque{T}"/>.</param>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the deque; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            var comparer = EqualityComparer<T>.Default;

            if (_head < _tail)
            {
                for (int i = _head; i < _tail; i++)
                    if (comparer.Equals(item, _array[i]))
                        return i - _head;
            }
            else
            {
                for (int i = _head; i < _array.Length; i++)
                    if (comparer.Equals(item, _array[i]))
                        return i - _head;

                for (int i = 0; i < _tail; i++)
                {
                    if (comparer.Equals(item, _array[i]))
                    {
                        return i + _array.Length - _head;
                    }
                }
            }

            return -1;
        }


        /// <summary>
        /// Copies the elements of the <see cref="Deque{T}"/> to an <see cref="Array"/>, starting at a 
        /// particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
        /// <see cref="Deque{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array"/> at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="array"/> is multidimensional. Or <paramref name="arrayIndex"/> is equal to 
        /// or greater than the length of <paramref name="array"/>. Or the number of elements in the 
        /// source <see cref="Deque{T}"/> is greater than the available space from 
        /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The <see cref="Deque{T}"/> is not modified. The order of the elements in the new array is 
        /// the same as the order of the elements from the head of the <see cref="Deque{T}"/> to 
        /// its tail.
        /// </para>
        /// <para>
        /// This method is an O(n) operation, where n is <see cref="Count"/>.
        /// </para>
        /// </remarks>
        public void CopyTo(T[] array, int arrayIndex) => ((ICollection)this).CopyTo(array, arrayIndex);


        /// <summary>
        /// Copies the elements of the <see cref="ICollection"/> to an <see cref="Array"/>, starting at 
        /// a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
        /// <see cref="ICollection"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array"/> at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than zero.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="array"/> is multidimensional. Or <paramref name="arrayIndex"/> is equal to
        /// or greater than the length of <paramref name="array"/>. Or the number of elements in the
        /// source <see cref="ICollection"/> is greater than the available space from 
        /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The type of the source <see cref="ICollection"/> cannot be cast automatically to the type of
        /// the destination <paramref name="array"/>.
        /// </exception>
        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be equal to or greater than 0.");
             
            if (array.Length > 0 && array.Length <= arrayIndex)
                throw new ArgumentException("Array index must be less than the length of the array.", "arrayIndex");

            if (_size > 0)
            {
                if (_head < _tail)
                {
                    Array.Copy(_array, _head, array, arrayIndex, _size);
                }
                else
                {
                    int headElements = _array.Length - _head;
                    Array.Copy(_array, _head, array, arrayIndex, headElements);
                    Array.Copy(_array, 0, array, arrayIndex + headElements, _tail);
                }
            }
        }


        /// <summary>
        /// Adds an object to the beginning of the <see cref="Deque{T}"/>. 
        /// </summary>
        /// <param name="item">
        /// The object to add to the <see cref="Deque{T}"/>. The value can be null for reference types.
        /// </param>
        /// <remarks>
        /// <para>
        /// If <see cref="Count"/> already equals the capacity, the capacity of the 
        /// <see cref="Deque{T}"/> is increased by automatically reallocating the internal array, and 
        /// the existing elements are copied to the new array before the new element is added. 
        /// </para>
        /// <para>
        /// If <see cref="Count"/> is less than the capacity of the internal array, this method is an 
        /// O(1) operation. If the internal array needs to be reallocated to accommodate the new 
        /// element, this method becomes an O(n) operation, where n is <see cref="Count"/>. 
        /// </para>
        /// </remarks>
        public void EnqueueHead(T item)
        {
            if (_size == _array.Length)
            {
                int capacity = _array.Length * 2;
                if (capacity < _array.Length + 4)
                    capacity = _array.Length + 4; // Grow capacity at least by 4.

                SetCapacity(capacity);
            }

            if (_head == 0)
                _head = _array.Length - 1;
            else
                _head--;

            _array[_head] = item;
            _size++;
            _version++;
        }


        /// <summary>
        /// Adds an object to the end of the <see cref="Deque{T}"/>. 
        /// </summary>
        /// <param name="item">
        /// The object to add to the <see cref="Deque{T}"/>. The value can be null for reference types.
        /// </param>
        /// <remarks>
        /// <para>
        /// If <see cref="Count"/> already equals the capacity, the capacity of the 
        /// <see cref="Deque{T}"/> is increased by automatically reallocating the internal array, and 
        /// the existing elements are copied to the new array before the new element is added. 
        /// </para>
        /// <para>
        /// If <see cref="Count"/> is less than the capacity of the internal array, this method is an 
        /// O(1) operation. If the internal array needs to be reallocated to accommodate the new 
        /// element, this method becomes an O(n) operation, where n is <see cref="Count"/>. 
        /// </para>
        /// </remarks>
        public void EnqueueTail(T item)
        {
            if (_size == _array.Length)
            {
                int capacity = _array.Length * 2;
                if (capacity < _array.Length + 4)
                    capacity = _array.Length + 4; // Grow capacity at least by 4.

                SetCapacity(capacity);
            }

            _array[_tail++] = item;
            if (_tail == _array.Length)
                _tail = 0;

            _size++;
            _version++;
        }


        /// <summary>
        /// Removes and returns the object at the beginning of the <see cref="Deque{T}"/>. 
        /// </summary>
        /// <returns>
        /// The object that is removed from the beginning of the <see cref="Deque{T}"/>. 
        /// </returns>
        /// <exception cref="InvalidOperationException">The <see cref="Deque{T}"/> is empty.</exception>
        /// <remarks>
        /// <para>
        /// If type <typeparamref name="T"/> is a reference type, <see langword="null"/> can be added to 
        /// the <see cref="Deque{T}"/> as a value. 
        /// </para>
        /// <para>
        /// This method is an O(1) operation. 
        /// </para>
        /// </remarks>
        public T DequeueHead()
        {
            if (_size == 0)
                throw new InvalidOperationException();

            T val = _array[_head];
            _array[_head] = default(T);
            _head++;
            if (_head == _array.Length)
                _head = 0;

            _size--;
            _version++;
            return val;
        }


        /// <summary>
        /// Removes and returns the object at the end of the <see cref="Deque{T}"/>. 
        /// </summary>
        /// <returns>
        /// The object that is removed from the end of the <see cref="Deque{T}"/>. 
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="Deque{T}"/> is empty.
        /// </exception>
        /// <remarks>
        /// <para>
        /// If type <typeparamref name="T"/> is a reference type, <see langword="null"/> can be added to
        /// the <see cref="Deque{T}"/> as a value. 
        /// </para>
        /// <para>
        /// This method is an O(1) operation. 
        /// </para>
        /// </remarks>
        public T DequeueTail()
        {
            if (_size == 0)
                throw new InvalidOperationException();

            if (_tail == 0)
                _tail = _array.Length - 1;
            else
                _tail--;

            T val = _array[_tail];
            _array[_tail] = default(T);
            _size--;
            _version++;
            return val;
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public Enumerator GetEnumerator() => new Enumerator(this);


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();


        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        /// <summary>
        /// Sets the capacity of the internal buffer.
        /// </summary>
        /// <param name="capacity">The capacity of the internal buffer.</param>
        private void SetCapacity(int capacity)
        {
            T[] array = new T[capacity];
            CopyTo(array, 0);

            _array = array;
            _head = 0;
            _tail = (_size == capacity) ? 0 : _size;
        }


        /// <summary>
        /// Copies the <see cref="Deque{T}"/> elements to a new array.
        /// </summary>
        /// <returns>A new array containing elements copied from the <see cref="Deque{T}"/>.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="Deque{T}"/> is not modified. The order of the elements in the new array is 
        /// the same as the order of the elements from the head of the <see cref="Deque{T}"/> to 
        /// its tail.
        /// </para>
        /// <para>
        /// This method is an O(n) operation, where n is <see cref="Count"/>.
        /// </para>
        /// </remarks>
        public T[] ToArray()
        {
            T[] array = new T[_size];
            CopyTo(array, 0);
            return array;
        }


        /// <summary>
        /// Sets the capacity to the actual number of elements in the <see cref="Deque{T}"/>, if that 
        /// number is less than 90 percent of current capacity.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method can be used to minimize a collection's memory overhead if no new elements will 
        /// be added to the collection. The cost of reallocating and copying a large 
        /// <see cref="Deque{T}"/> can be considerable, however, so the <see cref="TrimExcess"/> method 
        /// does nothing if the list is at more than 90 percent of capacity. This avoids incurring a 
        /// large reallocation cost for a relatively small gain. 
        /// </para>
        /// <para>
        /// This method is an O(n) operation, where n is <see cref="Count"/>. 
        /// </para>
        /// <para>
        /// To reset a <see cref="Deque{T}"/> to its initial state, call the <see cref="Clear"/> method 
        /// before calling TrimExcess method. Trimming an empty <see cref="Deque{T}"/> sets the capacity
        /// of the <see cref="Deque{T}"/> to the default capacity. 
        /// </para>
        /// </remarks>
        public void TrimExcess()
        {
            if (_size < (int)(0.9 * _array.Length))
                SetCapacity(_size);
        }


        #region ----- Unsupported ICollection<T> methods -----

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="item"/> was successfully removed from the 
        /// <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>. This method also returns 
        /// <see langword="false"/> if <paramref name="item"/> is not found in the original 
        /// <see cref="ICollection{T}"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ICollection{T}"/> is read-only.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException("Deque<T> does not support direct removal of items. Use DequeueHead or DequeueTail instead.");
        }

        #endregion
      

        #region ----- Unsupported IList<T> methods -----

        /// <summary>
        /// Inserts an item into the <see cref="IList{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which <paramref name="item"/> should be inserted.
        /// </param>
        /// <param name="item">
        /// The object to insert into the <see cref="IList{T}"/>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="IList{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="IList{T}"/> is read-only.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException("Deque<T> does not support direct insertion of items. EnqueueHead or EnqueueTail instead.");
        }


        /// <summary>
        /// Removes the item at the specified index from the <see cref="IList{T}"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="IList{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="IList{T}"/> is read-only.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException("Deque<T> does not support direct removal of items. Use DequeueHead or DequeueTail instead.");
        }
        #endregion
      
        #endregion
    }
}
