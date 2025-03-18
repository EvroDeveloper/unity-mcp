using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using SLZ;
using SLZ.Marrow;
using SLZ.Marrow.Warehouse;

namespace MCPServer.Editor.Commands
{
    public static class MarrowCommandHandler
    {
        public static object CreatePallet(JObject @params)
        {
            try
            {
                string palletName = (string)@params["pallet_name"];
                string palletAuthor = (string)@params["pallet_author"];

                if(string.IsNullOrEmpty(palletName))
                    return new { success = false, error = "Pallet Name cannot be empty"};
                
                if(string.IsNullOrEmpty(palletAuthor))
                    return new { success = false, error = "Pallet Author cannot be empty"};

                Pallet pallet = Pallet.CreatePallet(palletName, palletAuthor);
                if (AssetWarehouse.Instance.HasPallet(pallet.Barcode))
                {
                    Debug.LogError("Pallet with this barcode already exists.  Aborting.");
                    return new {
                        success = false;
                        message = "Pallet with this barcode already exists"
                    };
                }
                if (!Directory.Exists(MarrowSDK.GetMarrowAssetsPath(palletFolderName, pallet.Barcode.ID)))
                {
                    Directory.CreateDirectory(MarrowSDK.GetMarrowAssetsPath(palletFolderName, pallet.Barcode.ID));
                }
                string palletAssetFileName = pallet.GetAssetFilename();
                string crateAssetPath = MarrowSDK.GetMarrowAssetsPath(palletFolderName, pallet.Barcode.ID, palletAssetFileName);
                AssetDatabase.CreateAsset(pallet, crateAssetPath);
                EditorUtility.SetDirty(pallet);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AssetWarehouseWindow.ReloadWarehouse().Forget();

                return new 
                {
                    success = true,
                    message = $"Successfully created pallet at {palletPath}"
                    barcode = pallet.Barcode.ID;
                }
            }
            catch (System.Exception e)
            {
                return new { 
                    success = false, 
                    error = $"Failed to create pallet: {e.Message}", 
                    stackTrace = e.StackTrace 
                };
            }
        }

        public static object CreateScannable(JObject @params)
        {
            try
            {
                Barcode barcode = new Barcode((string)@params["pallet_barcode"]);
                string crateTitle = (string)@params["name"];
                string type = (string)@params["type"].ToUpper();
                string assetPath = (string)@params["asset_path"];

                !if(AssetWarehouse.Instance.TryGetPallet(barcode, out Pallet pallet))
                    return new { success = false, error = "Could not find a pallet with the given Barcode"};

                Type scannableType = typeof(Crate);

                switch (type)
                {
                    case("MONODISC"):
                        scannableType = typeof(MonoDisc);
                        break;
                    case("SPAWNABLE"):
                        scannableType = typeof(SpawnableCrate);
                        break;
                    case("LEVEL"):
                        scannableType = typeof(LevelCrate);
                        break;
                    default:
                        return new { success = false, error = "Invalid Type"};      
                }

                UnityEngine.Object crateAssetReference = AssetDatabase.LoadAssetAtPath(assetPath);

                if(crateAssetReference == null)
                {
                    return new { success = false, error = $"Could not find asset at {assetPath}"};
                }

                if(typeof(Crate).IsAssignableFrom(scannableType))
                {
                    Crate crate = Crate.CreateCrate(type, pallet, crateTitle, crateAssetReference);
                    string palletPath = AssetDatabase.GetAssetPath(pallet);
                    palletPath = System.IO.Path.GetDirectoryName(palletPath);
                    string crateAssetFilename = crate.GetAssetFilename();
                    string crateAssetPath = System.IO.Path.Combine(palletPath, crateAssetFilename);
                    AssetDatabase.CreateAsset(crate, crateAssetPath);
                    pallet.Crates.Add(crate);
                    return new { 
                        success = true, 
                        message = $"Successfully created {type}", 
                        barcode = crate.Barcode.ID
                    };
                }
                else if(typeof(DataCard).IsAssignableFrom(scannableType))
                {
                    DataCard dataCard = DataCard.CreateDataCard(scannableType, pallet, crateTitle)

                    if(dataCard is MonoDisc)
                    {
                        if(crateAssetReference is not AudioClip)
                        {
                            return new { 
                                success = false, 
                                error = "Asset Path did not reference AudioCip"
                            };
                        }
                        ((MonoDisc)dataCard).AudioClip = (AudioClip)crateAssetReference;
                    }

                    pallet.DataCards.Add(dataCard);
                    string palletPath = AssetDatabase.GetAssetPath(pallet);
                    palletPath = System.IO.Path.GetDirectoryName(palletPath);
                    string dataCardAssetFilename = dataCard.GetAssetFilename();
                    string assetPath = Path.Combine(palletPath, dataCardAssetFilename);
                    if (File.Exists(Path.Combine(Application.dataPath, assetPath)))
                    {
                        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    }

                    AssetDatabase.CreateAsset(dataCard, assetPath);
                    dataCard.GeneratePackedAssets(false);
                }

                EditorUtility.SetDirty(pallet);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AssetWarehouseWindow.ReloadWarehouse().Forget();
            }
            catch
            {
                return new { 
                    success = false, 
                    error = $"Failed to create {type}: {e.Message}", 
                    stackTrace = e.StackTrace 
                };
            }
        }

        public static object PackPallet(JObject @params)
        {
            Barcode barcode = new Barcode((string)@params["pallet_barcode"]);

            !if(AssetWarehouse.Instance.TryGetPallet(barcode, out Pallet outPallet))
                    return new { success = false, error = "Could not find a pallet with the given Barcode"};

            if(PalletPackerEditor.PackPallet(outPallet))
            {
                return new {
                    success = true,
                    message = "Pallet {barcode.ID} succesfully packed."
                };
            }
            else
            {
                return new {
                    success = false,
                    error = "Unable to pack Pallet. Do not try packing again until errors are fixed"
                };
            }
        }

        public static object GetPallets(JObject @params)
        {
            List<string> outputBarcodes = [];

            foreach(Pallet p in AssetWarehouse.Instance.GetPallets())
            {
                outputBarcodes.Add(p.Barcode.ID);
            }

            return new {
                success = true,
                barcodes = outputBarcodes
            };
        }

        public static object GetPalletInfo(JObject @params)
        {
            Barcode barcode = new Barcode((string)@params["pallet_barcode"]);

            !if(AssetWarehouse.Instance.TryGetPallet(barcode, out Pallet outPallet))
                    return new { success = false, error = "Could not find a pallet with the given Barcode"};

            return new {
                success = true,
                pallet = outPallet;
            };
        }
    }
}