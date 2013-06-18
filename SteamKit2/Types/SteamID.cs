﻿/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SteamKit2
{
    class BitVector64
    {
        private UInt64 data;

        public BitVector64()
        {
        }
        public BitVector64( UInt64 value )
        {
            data = value;
        }

        public UInt64 Data
        {
            get { return data; }
            set { data = value; }
        }

        public UInt64 this[ uint bitoffset, UInt64 valuemask ]
        {
            get
            {
                return ( data >> ( ushort )bitoffset ) & valuemask;
            }
            set
            {
                data = ( data & ~( valuemask << ( ushort )bitoffset ) ) | ( ( value & valuemask ) << ( ushort )bitoffset );
            }
        }
    }

    /// <summary>
    /// This 64bit structure is used for identifying various objects on the Steam network.
    /// </summary>
    [DebuggerDisplay( "{Render()}, {ConvertToUInt64()}" )]
    public class SteamID
    {
        BitVector64 steamid;

        static Regex SteamIDRegex = new Regex(
            @"STEAM_(?<universe>[0-5]):(?<authserver>[0-1]):(?<accountid>\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase );

        /// <summary>
        /// The account instance value when representing all instanced <see cref="SteamID">SteamIDs</see>.
        /// </summary>
        public const uint AllInstances = 0;
        /// <summary>
        /// The account instance value for a desktop <see cref="SteamID"/>.
        /// </summary>
        public const uint DesktopInstance = 1;
        /// <summary>
        /// The account instance value for a console <see cref="SteamID"/>.
        /// </summary>
        public const uint ConsoleInstance = 2;
        /// <summary>
        /// The account instance for mobile or web based <see cref="SteamID">SteamIDs</see>.
        /// </summary>
        public const uint WebInstance = 4;

        /// <summary>
        /// Masking vlaue used for the account id.
        /// </summary>
        public const uint AccountIDMask = 0xFFFFFFFF;
        /// <summary>
        /// Masking value used for packing chat instance flags into a <see cref="SteamID"/>.
        /// </summary>
        public const uint AccountInstanceMask = 0x000FFFFF;


        /// <summary>
        /// Represents various flags a chat <see cref="SteamID"/> may have, packed into its instance.
        /// </summary>
        [Flags]
        public enum ChatInstanceFlags : uint
        {
            /// <summary>
            /// This flag is set for clan based chat <see cref="SteamID">SteamIDs</see>.
            /// </summary>
            Clan = ( AccountInstanceMask + 1 ) >> 1,
            /// <summary>
            /// This flag is set for lobby based chat <see cref="SteamID">SteamIDs</see>.
            /// </summary>
            Lobby = ( AccountInstanceMask + 1 ) >> 2,
            /// <summary>
            /// This flag is set for matchmaking lobby based chat <see cref="SteamID">SteamIDs</see>.
            /// </summary>
            MMSLobby = ( AccountInstanceMask + 1 ) >> 3,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class.
        /// </summary>
        public SteamID()
            : this( 0 )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class.
        /// </summary>
        /// <param name="unAccountID">The account ID.</param>
        /// <param name="eUniverse">The universe.</param>
        /// <param name="eAccountType">The account type.</param>
        public SteamID( UInt32 unAccountID, EUniverse eUniverse, EAccountType eAccountType )
            : this()
        {
            Set( unAccountID, eUniverse, eAccountType );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class.
        /// </summary>
        /// <param name="unAccountID">The account ID.</param>
        /// <param name="unInstance">The instance.</param>
        /// <param name="eUniverse">The universe.</param>
        /// <param name="eAccountType">The account type.</param>
        public SteamID( UInt32 unAccountID, UInt32 unInstance, EUniverse eUniverse, EAccountType eAccountType )
            : this()
        {
            InstancedSet( unAccountID, unInstance, eUniverse, eAccountType );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class.
        /// </summary>
        /// <param name="id">The 64bit integer to assign this SteamID from.</param>
        public SteamID( UInt64 id )
        {
            this.steamid = new BitVector64( id );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class from a Steam2 "STEAM_" rendered form.
        /// This constructor assumes the rendered SteamID is in the public universe.
        /// </summary>
        /// <param name="steamId">A "STEAM_" rendered form of the SteamID.</param>
        public SteamID( string steamId )
            : this ( steamId, EUniverse.Public )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamID"/> class from a Steam2 "STEAM_" rendered form and universe.
        /// </summary>
        /// <param name="steamId">A "STEAM_" rendered form of the SteamID.</param>
        /// <param name="eUniverse">The universe the SteamID belongs to.</param>
        public SteamID( string steamId, EUniverse eUniverse )
            : this()
        {
            SetFromString( steamId, eUniverse );
        }


        /// <summary>
        /// Sets the various components of this SteamID instance.
        /// </summary>
        /// <param name="unAccountID">The account ID.</param>
        /// <param name="eUniverse">The universe.</param>
        /// <param name="eAccountType">The account type.</param>
        public void Set( UInt32 unAccountID, EUniverse eUniverse, EAccountType eAccountType )
        {
            this.AccountID = unAccountID;
            this.AccountUniverse = eUniverse;
            this.AccountType = eAccountType;

            if ( eAccountType == EAccountType.Clan )
            {
                this.AccountInstance = 0;
            }
            else
            {
                this.AccountInstance = DesktopInstance;
            }
        }

        /// <summary>
        /// Sets the various components of this SteamID instance.
        /// </summary>
        /// <param name="unAccountID">The account ID.</param>
        /// <param name="unInstance">The instance.</param>
        /// <param name="eUniverse">The universe.</param>
        /// <param name="eAccountType">The account type.</param>
        public void InstancedSet( UInt32 unAccountID, UInt32 unInstance, EUniverse eUniverse, EAccountType eAccountType )
        {
            this.AccountID = unAccountID;
            this.AccountUniverse = eUniverse;
            this.AccountType = eAccountType;
            this.AccountInstance = unInstance;
        }


        /// <summary>
        /// Sets the various components of this SteamID from a Steam2 "STEAM_" rendered form and universe.
        /// </summary>
        /// <param name="steamId">A "STEAM_" rendered form of the SteamID.</param>
        /// <param name="eUniverse">The universe the SteamID belongs to.</param>
        /// <returns><c>true</c> if this instance was successfully assigned; otherwise, <c>false</c> if the given string was in an invalid format.</returns>
        public bool SetFromString( string steamId, EUniverse eUniverse )
        {
            if ( string.IsNullOrEmpty( steamId ) )
                return false;

            Match m = SteamIDRegex.Match( steamId );

            if ( !m.Success )
                return false;

            uint accId, authServer;
            if ( !uint.TryParse( m.Groups[ "accountid" ].Value, out accId ) || 
                 !uint.TryParse( m.Groups[ "authserver" ].Value, out authServer ) )
                return false;

            this.AccountUniverse = eUniverse;
            this.AccountInstance = 1;
            this.AccountType = EAccountType.Individual;
            this.AccountID = ( accId << 1 ) | authServer;

            return true;
        }

        /// <summary>
        /// Sets the various components of this SteamID from a 64bit integer form.
        /// </summary>
        /// <param name="ulSteamID">The 64bit integer to assign this SteamID from.</param>
        public void SetFromUInt64( UInt64 ulSteamID )
        {
            this.steamid.Data = ulSteamID;
        }

        /// <summary>
        /// Converts this SteamID into it's 64bit integer form.
        /// </summary>
        /// <returns>A 64bit integer representing this SteamID.</returns>
        public UInt64 ConvertToUInt64()
        {
            return this.steamid.Data;
        }

        /// <summary>
        /// Returns a static account key used for grouping accounts with differing instances.
        /// </summary>
        /// <returns>A 64bit static account key.</returns>
        public ulong GetStaticAccountKey()
        {
            return ( ( ulong )AccountUniverse << 56 ) + ( ( ulong )AccountType << 52 ) + AccountID;
        }




        /// <summary>
        /// Gets a value indicating whether this instance is a blank anonymous account
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a blank anon account; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlankAnonAccount
        {
            get { return this.AccountID == 0 && IsAnonAccount && this.AccountInstance == 0; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is a game server account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a game server account; otherwise, <c>false</c>.
        /// </value>
        public bool IsGameServerAccount
        {
            get { return this.AccountType == EAccountType.GameServer || this.AccountType == EAccountType.AnonGameServer; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a persistent game server account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a persistent game server account; otherwise, <c>false</c>.
        /// </value>
        public bool IsPersistentGameServerAccount
        {
            get { return this.AccountType == EAccountType.GameServer; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is an anonymous game server account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is an anon game server account; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnonGameServerAccount
        {
            get { return this.AccountType == EAccountType.AnonGameServer; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a content server account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a content server account; otherwise, <c>false</c>.
        /// </value>
        public bool IsContentServerAccount
        {
            get { return this.AccountType == EAccountType.ContentServer; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is a clan account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a clan account; otherwise, <c>false</c>.
        /// </value>
        public bool IsClanAccount
        {
            get { return this.AccountType == EAccountType.Clan; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is a chat account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a chat account; otherwise, <c>false</c>.
        /// </value>
        public bool IsChatAccount
        {
            get { return this.AccountType == EAccountType.Chat; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is a lobby.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a lobby; otherwise, <c>false</c>.
        /// </value>
        public bool IsLobby
        {
            get {
                return ( this.AccountType == EAccountType.Chat ) &&
                    ( ( this.AccountInstance & ( uint )ChatInstanceFlags.Lobby ) > 0 );
            }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is an individual account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is an individual account; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndividualAccount
        {
            get { return this.AccountType == EAccountType.Individual || this.AccountType == EAccountType.ConsoleUser; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is an anonymous account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is an anon account; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnonAccount
        {
            get { return this.AccountType == EAccountType.AnonUser || this.AccountType == EAccountType.AnonGameServer; }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is an anonymous user account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is an anon user account; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnonUserAccount
        {
            get { return this.AccountType == EAccountType.AnonUser; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a console user account.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a console user account; otherwise, <c>false</c>.
        /// </value>
        public bool IsConsoleUserAccount
        {
            get { return this.AccountType == EAccountType.ConsoleUser; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                if ( this.AccountType <= EAccountType.Invalid || this.AccountType >= EAccountType.Max )
                    return false;

                if ( this.AccountUniverse <= EUniverse.Invalid || this.AccountUniverse >= EUniverse.Max )
                    return false;

                if ( this.AccountType == EAccountType.Individual )
                {
                    if ( this.AccountID == 0 || this.AccountInstance > WebInstance )
                        return false;
                }

                if ( this.AccountType == EAccountType.Clan )
                {
                    if ( this.AccountID == 0 || this.AccountInstance != 0 )
                        return false;
                }

                if ( this.AccountType == EAccountType.GameServer )
                {
                    if ( this.AccountID == 0 )
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets or sets the account id.
        /// </summary>
        /// <value>
        /// The account id.
        /// </value>
        public UInt32 AccountID
        {
            get
            {
                return ( UInt32 )steamid[ 0, 0xFFFFFFFF ];
            }
            set
            {
                steamid[ 0, 0xFFFFFFFF ] = value;
            }
        }
        /// <summary>
        /// Gets or sets the account instance.
        /// </summary>
        /// <value>
        /// The account instance.
        /// </value>
        public UInt32 AccountInstance
        {
            get
            {
                return ( UInt32 )steamid[ 32, 0xFFFFF ];
            }
            set
            {
                steamid[ 32, 0xFFFFF ] = ( UInt64 )value;
            }
        }
        /// <summary>
        /// Gets or sets the account type.
        /// </summary>
        /// <value>
        /// The account type.
        /// </value>
        public EAccountType AccountType
        {
            get
            {
                return ( EAccountType )steamid[ 52, 0xF ];
            }
            set
            {
                steamid[ 52, 0xF ] = ( UInt64 )value;
            }
        }
        /// <summary>
        /// Gets or sets the account universe.
        /// </summary>
        /// <value>
        /// The account universe.
        /// </value>
        public EUniverse AccountUniverse
        {
            get
            {
                return ( EUniverse )steamid[ 56, 0xFF ];
            }
            set
            {
                steamid[ 56, 0xFF ] = ( UInt64 )value;
            }
        }

        /// <summary>
        /// Renders this instance into it's Steam2 "STEAM_" or Steam3 represenation.
        /// </summary>
        /// <param name="steam3">If set to <c>true</c>, the Steam3 rendering will be returned; otherwise, the Steam2 STEAM_ rendering.</param>
        /// <returns>
        /// A string Steam2 "STEAM_" representation of this SteamID, or a Steam3 representation.
        /// </returns>
        public string Render( bool steam3 = false )
        {
            if ( steam3 )
                return RenderSteam3();

            return RenderSteam2();
        }

        string RenderSteam2()
        {
            switch ( AccountType )
            {
                case EAccountType.Invalid:
                case EAccountType.Individual:
                    if ( AccountUniverse <= EUniverse.Public )
                        return String.Format( "STEAM_0:{0}:{1}", AccountID & 1, AccountID >> 1 );
                    else
                        return String.Format( "STEAM_{2}:{0}:{1}", AccountID & 1, AccountID >> 1, ( int )AccountUniverse );
                default:
                    return Convert.ToString( this );
            }
        }
        string RenderSteam3()
        {
            switch ( AccountType )
            {
                case EAccountType.AnonGameServer:
                    return string.Format( "[A:{0}:{1}:{2}]", ( uint )AccountUniverse, AccountID, AccountInstance );

                case EAccountType.GameServer:
                    return string.Format( "[G:{0}:{1}]", ( uint )AccountUniverse, AccountID );

                case EAccountType.Multiseat:
                    return string.Format( "[M:{0}:{1}:{2}]", ( uint )AccountUniverse, AccountID, AccountInstance );

                case EAccountType.Pending:
                    return string.Format( "[P:{0}:{1}]", ( uint )AccountUniverse, AccountID );

                case EAccountType.ContentServer:
                    return string.Format( "[C:{0}:{1}]", ( uint )AccountUniverse, AccountID );

                case EAccountType.Clan:
                    return string.Format( "[g:{0}:{1}]", ( uint )AccountUniverse, AccountID );

                case EAccountType.Chat:
                    {
                        if ( ( ( ChatInstanceFlags )AccountInstance ).HasFlag( ChatInstanceFlags.Clan ) )
                            return string.Format( "[c:{0}:{1}]", ( uint )AccountUniverse, AccountID );

                        else if ( ( ( ChatInstanceFlags )AccountInstance ).HasFlag( ChatInstanceFlags.Lobby ) )
                            return string.Format( "[L:{0}:{1}]", ( uint )AccountUniverse, AccountID );

                        else
                            return string.Format( "[T:{0}:{1}]", ( uint )AccountUniverse, AccountID );
                    }

                case EAccountType.Invalid:
                    return string.Format( "[I:{0}:{1}]", ( uint )AccountUniverse, AccountID );

                case EAccountType.Individual:
                    {
                        if ( AccountInstance == DesktopInstance )
                            return string.Format( "[U:{0}:{1}]", ( uint )AccountUniverse, AccountID );

                        else
                            return string.Format( "[U:{0}:{1}:{2}]", ( uint )AccountUniverse, AccountID, AccountInstance );
                    }

                case EAccountType.AnonUser:
                    return string.Format( "[a:{0}:{1}]", ( uint )AccountUniverse, AccountID );

                default:
                    return string.Format( "[i:{0}:{1}]", ( uint )AccountUniverse, AccountID );
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            // for compatibility, we will always return a Steam2 rendering when ToString()'d
            return Render( false );
        }


        /// <summary>
        /// Performs an implicit conversion from <see cref="SteamKit2.SteamID"/> to <see cref="System.UInt64"/>.
        /// </summary>
        /// <param name="sid">The SteamID.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator UInt64( SteamID sid )
        {
            return sid.steamid.Data;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.UInt64"/> to <see cref="SteamKit2.SteamID"/>.
        /// </summary>
        /// <param name="id">A 64bit integer representing the SteamID.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator SteamID( UInt64 id )
        {
            return new SteamID( id );
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals( System.Object obj )
        {
            if ( obj == null )
                return false;

            SteamID sid = obj as SteamID;
            if ( ( System.Object )sid == null )
                return false;

            return steamid.Data == sid.steamid.Data;
        }

        /// <summary>
        /// Determines whether the specified <see cref="SteamID"/> is equal to this instance.
        /// </summary>
        /// <param name="sid">The <see cref="SteamID"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="SteamID"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals( SteamID sid )
        {
            if ( ( object )sid == null )
                return false;

            return steamid.Data == sid.steamid.Data;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">The left side SteamID.</param>
        /// <param name="b">The right side SteamID.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==( SteamID a, SteamID b )
        {
            if ( System.Object.ReferenceEquals( a, b ) )
                return true;

            if ( ( ( object )a == null ) || ( ( object )b == null ) )
                return false;

            return a.steamid.Data == b.steamid.Data;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">The left side SteamID.</param>
        /// <param name="b">The right side SteamID.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=( SteamID a, SteamID b )
        {
            return !( a == b );
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return steamid.Data.GetHashCode();
        }

    }
}
