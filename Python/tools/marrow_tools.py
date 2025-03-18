from mcp.server.fastmcp import FastMCP, Context
from typing import Optional
from unity_connection import get_unity_connection

  #    "   - `create_monodisc(pallet_barcode, name, audio_path)` - Create a MonoDisc datacard"
   #     "   - `create_spawnable(pallet_barcode, name, prefab_path)` - Create a Spawnable"
       # "   - `pack_pallet(pallet_barcode)` - Packs a pallet"
        #"   - `get_pallet_list(search_pattern)` - List of barcodes of Pallets"

def register_marrow_tools(mcp: FastMCP):
    """Register all Marrow control tools with the MCP server."""

    @mcp.tool()
    def create_pallet(
        ctx: Context,
        pallet_name: str,
        pallet_author: str
    ) -> str:
        """Create a Marrow Pallet with the specified name and author.
        
        Args:
            ctx: The MCP context
            pallet_name: Name of the Pallet to create
            pallet_author: Author of the Pallet to create
        
        Returns:
            Barcode of the pallet
        """
        try:
            response = get_unity_connection().send_command("MARROW_CREATE_PALLET", {
                "pallet_name": pallet_name,
                "pallet_author": pallet_author
            })
            return response
        except Exception as e:
            return {"error": f"Failed to create Pallet: {str(e)}"}

    @mcp.tool()
    def create_monodisc(
        ctx: Context,
        pallet_barcode: str,
        name: str,
        audio_path: str,
    ) -> str:
        """Create a Marrow MonoDisc from an AudioClip.
        
        Args:
            ctx: The MCP context
            pallet_barcode: The barcode of the Pallet to add the MonoDisc to
            name: Name of the MonoDisc
            audio_path: Path of the AudioClip to put in the MonoDisc
            
        Returns:
            Barcode of the MonoDisc
        """
        try:
            response = get_unity_connection().send_command("MARROW_CREATE_SCANNABLE", {
                "pallet_barcode": pallet_barcode,
                "name": name,
                "type": "MONODISC",
                "asset_path": audio_path
            })
            return response
        except Exception as e:
            return {"error": f"Failed to create MonoDisc: {str(e)}"}
    
    @mcp.tool()
    def create_spawnable(
        ctx: Context,
        pallet_barcode: str,
        name: str,
        prefab_path: str,
    ) -> str:
        """Create a Marrow Spawnable from a Prefab.
        
        Args:
            ctx: The MCP context
            pallet_barcode: The barcode of the Pallet to add the Spawnable to
            name: Name of the Spawnable
            prefab_path: Path of the AudioClip to put in the Spawnable
            
        Returns:
            Barcode of the Spawnable
        """
        try:
            response = get_unity_connection().send_command("MARROW_CREATE_SCANNABLE", {
                "pallet_barcode": pallet_barcode,
                "name": name,
                "type": "SPAWNABLE",
                "asset_path": prefab_path
            })
            return response
        except Exception as e:
            return {"error": f"Failed to create Spawnable: {str(e)}"}

    @mcp.tool()
    def pack_pallet(
        ctx: Context,
        pallet_barcode: str
    ) -> str:
        """Packs the specified Pallet into a Marrow Mod
        
        Args:
            ctx: The MCP Context
            pallet_barcode: The barcode of the Pallet to pack
        """
        try:
            response = get_unity_connection().send_command("MARROW_PACK_PALLET", {
                "pallet_barcode": pallet_barcode
            })
            return response
        except Exception as e:
            return {"error": f"Failed to Pack Pallet: {str(e)}"}

    @mcp.tool()
    def get_pallet_list(ctx: Context) -> str:
        """Gets a list of Pallet Barcodes in the project
        
        Args:
            ctx: The MCP Context
            
        Returns:
            List of Barcode strings"""

        try:
            response = get_unity_connection().send_command("MARROW_GET_PALLETS")
            return response
        except Exception as e:
            return {"error": f"Failed to get Pallets list: {str(e)}"}
    
    @mcp.tool()
    def get_pallet_info(
        ctx: Context,
        pallet_barcode: str
    ) -> str:
        """Gets the properties of a specific Pallet
        
        Args:
            ctx: The MCP Context
            pallet_barcode: Barcode of the pallet
            
        Returns:
            The properties of the Pallet object"""
        
        try:
            response = get_unity_connection().send_command("MARROW_GET_PALLET_INFO", {
                "pallet_barcode": pallet_barcode
            })
            return response
        except Exception as e:
            return {"error": f"Failed to get Pallet info: {str(e)}"}