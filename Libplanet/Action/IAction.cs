using System.Collections.Immutable;

namespace Libplanet.Action
{
    /// <summary>
    /// An in-game action.  Every action should be replayable, because
    /// multiple nodes in a network should execute an action and get the same
    /// result.
    /// <para>A &#x201c;class&#x201d; which implements this interface is
    /// analogous to a function, and its instance is analogous to a
    /// <a href="https://en.wikipedia.org/wiki/Partial_application">partial
    /// function application</a>, in other words, a function with some bound
    /// arguments.  Those parameters that will be bound at runtime should be
    /// represented as fields or properties in an action class, and bound
    /// argument values to these parameters should be received through
    /// a constructor parameters of that class.</para>
    /// </summary>
    /// <example>
    /// The following example shows how to implement an action of three types
    /// of in-game logic:
    /// <code><![CDATA[
    /// using System;
    /// using System.Collections.Immutable;
    /// using Libplanet;
    /// using Libplanet.Action;
    ///
    /// public class MyAction : IAction
    /// {
    ///     // Declare an enum type to distinguish types of in-game logic.
    ///     public enum ActType { CreateCharacter, Attack, Heal }
    ///
    ///     // Declare properties (or fields) to store "bound" argument values.
    ///     public ActType Type { get; private set; }
    ///     public Address TargetAddress { get; private set; }
    ///
    ///     // Action must has a public parameterless constructor.
    ///     // Usually this is used only by Libplanet's internals.
    ///     public MyAction() {}
    ///
    ///     // Take argument values to "bind" through constructor parameters.
    ///     public MyAction(ActType type, Address targetAddress)
    ///     {
    ///         Type = type;
    ///         TargetAddress = targetAddress;
    ///     }
    ///
    ///     // The main game logic belongs to here.  It takes the
    ///     // previous states through its parameter named context,
    ///     // and is offered "bound" argument values through
    ///     // its own properties (or fields).
    ///     IAccountStateDelta IAction.Execute(IActionContext context)
    ///     {
    ///         // Gets the state immediately before this action is executed.
    ///         // ImmutableDictionary<string, uint> is just for example,
    ///         // As far as it is serializable, you can store any types.
    ///         // (We recommend to use immutable types though.)
    ///         var state = (ImmutableDictionary<string, uint>)
    ///             context.PreviousStates.GetState(TargetAddress);
    ///
    ///         // This variable purposes to store the state
    ///         // right after this action finishes.
    ///         ImmutableDictionary<string, uint> nextState;
    ///
    ///         // Does different things depending on the action's type.
    ///         // This way is against the common principals of programming
    ///         // as it is just an example.  You could compare this with
    ///         // a better example of PolymorphicAction<T> class.
    ///         switch (Type)
    ///         {
    ///             case ActType.CreateCharacter:
    ///                 if (!TargetAddress.Equals(context.Signer))
    ///                     throw new Exception(
    ///                         "TargetAddress of CreateCharacter action " +
    ///                         "only can be the same address to the " +
    ///                         "Transaction<T>.Signer.");
    ///                 else if (!(state is null))
    ///                     throw new Exception(
    ///                         "Character was already created.");
    ///
    ///                 nextState = ImmutableDictionary<string, uint>.Empty
    ///                     .Add("hp", 20);
    ///                 break;
    ///
    ///             case ActType.Attack:
    ///                 nextState =
    ///                     state.SetItem("hp", Math.Max(state["hp"] - 5, 0));
    ///                 break;
    ///
    ///             case ActType.Heal:
    ///                 nextState =
    ///                     state.SetItem("hp", Math.Min(state["hp"] + 5, 20));
    ///                 break;
    ///
    ///             default:
    ///                 throw new Exception(
    ///                     "Properties are not properly initialized.");
    ///         }
    ///
    ///         // Builds a delta (dirty) from previous to next states, and
    ///         // returns it.
    ///         return context.PreviousStates.SetState(TargetAddress,
    ///             nextState);
    ///     }
    ///
    ///     // Serializes its "bound arguments" so that they are transmitted
    ///     // over network or stored to the persistent storage.
    ///     // It uses .NET's built-in serialization mechanism.
    ///     IImmutableDictionary<string, object> IAction.PlainValue =>
    ///         ImmutableDictionary<string, object>.Empty
    ///             .Add("type", Type)
    ///             .Add("target_address", TargetAddress);
    ///
    ///     // Deserializes "bound arguments".  That is, it is inverse
    ///     // of PlainValue property.
    ///     void IAction.LoadPlainValue(
    ///         IImmutableDictionary<string, object> plainValue)
    ///     {
    ///         Type = (ActType)plainValue["type"];
    ///         TargetAddress = (Address)plainValue["target_address"];
    ///     }
    /// }
    /// ]]></code>
    /// <para>Note that the above example has several bad practices.
    /// Compare this example with <see cref="PolymorphicAction{T}"/>'s
    /// example.</para>
    /// </example>
    public interface IAction
    {
        /// <summary>
        /// Serializes values bound to an action, which is held by properties
        /// (or fields) of an action, so that they can be transmitted over
        /// network or saved to persistent storage.
        /// <para>Serialized values are deserialized by <see
        /// cref="LoadPlainValue(IImmutableDictionary{string,object})"/> method
        /// later.</para>
        /// <para>It uses <a href=
        /// "https://docs.microsoft.com/en-us/dotnet/standard/serialization/"
        /// >.NET's built-in serialization mechanism</a>.</para>
        /// </summary>
        /// <returns>A value which encodes this action's bound values (held
        /// by properties or fields).  It has to be <a href=
        /// "https://docs.microsoft.com/en-us/dotnet/standard/serialization/"
        /// >serializable</a>.</returns>
        /// <seealso
        /// cref="LoadPlainValue(IImmutableDictionary{string, object})"/>
        IImmutableDictionary<string, object> PlainValue { get; }

        /// <summary>
        /// Deserializes serialized data (i.e., data <see cref="PlainValue"/>
        /// property made), and then fills this action's properties (or fields)
        /// with the deserialized values.
        /// </summary>
        /// <param name="plainValue">Data (made by <see cref="PlainValue"/>
        /// property) to be deserialized and assigned to this action's
        /// properties (or fields).</param>
        /// <seealso cref="PlainValue"/>
        void LoadPlainValue(IImmutableDictionary<string, object> plainValue);

        /// <summary>
        /// Executes the main game logic of an action.  This should be
        /// <em>deterministic</em>.
        /// <para>Through the <paramref name="context"/> object,
        /// it receives information such as a transaction signer,
        /// its states immediately before the execution,
        /// and a deterministic random seed.</para>
        /// <para>Other &#x201c;bound&#x201d; information resides in the action
        /// object in itself, as its properties (or fields).</para>
        /// <para>A returned <see cref="AddressStateMap"/> object functions as
        /// a delta which shifts from previous states to next states.</para>
        /// </summary>
        /// <param name="context">A context object containing addresses that
        /// signed the transaction, states immediately before the execution,
        /// and a PRNG object which produces deterministic random numbers.
        /// See <see cref="IActionContext"/> for details.</param>
        /// <returns>A map of changed states (so-called "dirty").</returns>
        /// <remarks>This method should be deterministic:
        /// for structurally (member-wise) equal actions and <see
        /// cref="IActionContext"/>s, the same result should be returned.
        /// <para>For randomness, <em>never</em> use <see cref="System.Random"/>
        /// nor any other PRNGs provided by other than Libplanet.
        /// Use <see cref="IActionContext.Random"/> instead.</para>
        /// <para>Also do not perform I/O operations such as file system access
        /// or networking.  These bring an action indeterministic.</para>
        /// </remarks>
        /// <seealso cref="IActionContext"/>
        IAccountStateDelta Execute(IActionContext context);
    }
}
