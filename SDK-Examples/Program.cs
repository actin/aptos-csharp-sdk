// See https://aka.ms/new-console-template for more information

using Aptos.Sample;
using Aptos.Rest.Model;
using Newtonsoft.Json;

Console.WriteLine("Hello, World!");


var json = """
        {
        "version": "659527",
        "hash": "0xd09ddcbe132c6b2a7fd7c70880069106140443bd96ff7bcea675469bb7fb4597",
        "state_change_hash": "0xfe32033aed9317b418b6a99eaee22f168eaed48666ee3adb5aa01d411e12faa1",
        "event_root_hash": "0x5756a650ff3102d8dbe1f3b9b0bf4d76cc46928407fd0747c3c51abb359d6955",
        "state_checkpoint_hash": null,
        "gas_used": "720",
        "success": true,
        "vm_status": "Executed successfully",
        "accumulator_root_hash": "0x270f1311ae046f59277ee60d38ef43a8fcd3920d0e9f08509fc4a698897e005e",
        "changes": [
            {
                "address": "0x39274b41595692e776b9ebef0911e603980c590c1bbbbb62d1c4b94fb866e803",
                "state_key_hash": "0x273cb8033482260a9160b33934b665c0e8084c8d8531e979d7b830ed4809b438",
                "data": {
                    "type": "0x1::object::ObjectCore",
                    "data": {
                        "allow_ungated_transfer": false,
                        "guid_creation_num": "1125899906842626",
                        "owner": "0x85cafaffb4abd2aef440c7b167ca245b1d7db96d96b22e0553d3173e5491a3d0",
                        "transfer_events": {
                            "counter": "0",
                            "guid": {
                                "id": {
                                    "addr": "0x39274b41595692e776b9ebef0911e603980c590c1bbbbb62d1c4b94fb866e803",
                                    "creation_num": "1125899906842624"
                                }
                            }
                        }
                    }
                },
                "type": "write_resource"
            },
            {
                "address": "0x39274b41595692e776b9ebef0911e603980c590c1bbbbb62d1c4b94fb866e803",
                "state_key_hash": "0x273cb8033482260a9160b33934b665c0e8084c8d8531e979d7b830ed4809b438",
                "data": {
                    "type": "0x4::aptos_token::AptosCollection",
                    "data": {
                        "mutable_description": true,
                        "mutable_token_description": true,
                        "mutable_token_name": true,
                        "mutable_token_properties": true,
                        "mutable_token_uri": true,
                        "mutable_uri": true,
                        "mutator_ref": {
                            "vec": [
                                {
                                    "self": "0x39274b41595692e776b9ebef0911e603980c590c1bbbbb62d1c4b94fb866e803"
                                }
                            ]
                        },
                        "royalty_mutator_ref": {
                            "vec": [
                                {
                                    "inner": {
                                        "self": "0x39274b41595692e776b9ebef0911e603980c590c1bbbbb62d1c4b94fb866e803"
                                    }
                                }
                            ]
                        },
                        "tokens_burnable_by_creator": true,
                        "tokens_freezable_by_creator": true
                    }
                },
                "type": "write_resource"
            },
            {
                "address": "0x39274b41595692e776b9ebef0911e603980c590c1bbbbb62d1c4b94fb866e803",
                "state_key_hash": "0x273cb8033482260a9160b33934b665c0e8084c8d8531e979d7b830ed4809b438",
                "data": {
                    "type": "0x4::collection::Collection",
                    "data": {
                        "creator": "0x85cafaffb4abd2aef440c7b167ca245b1d7db96d96b22e0553d3173e5491a3d0",
                        "description": "Alice's simple collection",
                        "mutation_events": {
                            "counter": "0",
                            "guid": {
                                "id": {
                                    "addr": "0x39274b41595692e776b9ebef0911e603980c590c1bbbbb62d1c4b94fb866e803",
                                    "creation_num": "1125899906842625"
                                }
                            }
                        },
                        "name": "Alice's",
                        "uri": "http://ntroi.com"
                    }
                },
                "type": "write_resource"
            },
            {
                "address": "0x39274b41595692e776b9ebef0911e603980c590c1bbbbb62d1c4b94fb866e803",
                "state_key_hash": "0x273cb8033482260a9160b33934b665c0e8084c8d8531e979d7b830ed4809b438",
                "data": {
                    "type": "0x4::collection::ConcurrentSupply",
                    "data": {
                        "current_supply": {
                            "max_value": "1",
                            "value": "1"
                        },
                        "total_minted": {
                            "max_value": "18446744073709551615",
                            "value": "1"
                        }
                    }
                },
                "type": "write_resource"
            },
            {
                "address": "0x39274b41595692e776b9ebef0911e603980c590c1bbbbb62d1c4b94fb866e803",
                "state_key_hash": "0x273cb8033482260a9160b33934b665c0e8084c8d8531e979d7b830ed4809b438",
                "data": {
                    "type": "0x4::royalty::Royalty",
                    "data": {
                        "denominator": "1",
                        "numerator": "0",
                        "payee_address": "0x85cafaffb4abd2aef440c7b167ca245b1d7db96d96b22e0553d3173e5491a3d0"
                    }
                },
                "type": "write_resource"
            },
            {
                "address": "0x85cafaffb4abd2aef440c7b167ca245b1d7db96d96b22e0553d3173e5491a3d0",
                "state_key_hash": "0xacb1ab6aaa237d97fdab6c951d8541f69f06e751608f369e01c9c4412f1a13d4",
                "data": {
                    "type": "0x1::coin::CoinStore<0x1::aptos_coin::AptosCoin>",
                    "data": {
                        "coin": {
                            "value": "29857700"
                        },
                        "deposit_events": {
                            "counter": "1",
                            "guid": {
                                "id": {
                                    "addr": "0x85cafaffb4abd2aef440c7b167ca245b1d7db96d96b22e0553d3173e5491a3d0",
                                    "creation_num": "2"
                                }
                            }
                        },
                        "frozen": false,
                        "withdraw_events": {
                            "counter": "0",
                            "guid": {
                                "id": {
                                    "addr": "0x85cafaffb4abd2aef440c7b167ca245b1d7db96d96b22e0553d3173e5491a3d0",
                                    "creation_num": "3"
                                }
                            }
                        }
                    }
                },
                "type": "write_resource"
            },
            {
                "address": "0x85cafaffb4abd2aef440c7b167ca245b1d7db96d96b22e0553d3173e5491a3d0",
                "state_key_hash": "0x2241e7b849b508d0e5381eb4b20fa9c116f2ba9e623dc937f389e0ea78be4e8f",
                "data": {
                    "type": "0x1::account::Account",
                    "data": {
                        "authentication_key": "0x85cafaffb4abd2aef440c7b167ca245b1d7db96d96b22e0553d3173e5491a3d0",
                        "coin_register_events": {
                            "counter": "1",
                            "guid": {
                                "id": {
                                    "addr": "0x85cafaffb4abd2aef440c7b167ca245b1d7db96d96b22e0553d3173e5491a3d0",
                                    "creation_num": "0"
                                }
                            }
                        },
                        "guid_creation_num": "4",
                        "key_rotation_events": {
                            "counter": "0",
                            "guid": {
                                "id": {
                                    "addr": "0x85cafaffb4abd2aef440c7b167ca245b1d7db96d96b22e0553d3173e5491a3d0",
                                    "creation_num": "1"
                                }
                            }
                        },
                        "rotation_capability_offer": {
                            "for": {
                                "vec": []
                            }
                        },
                        "sequence_number": "2",
                        "signer_capability_offer": {
                            "for": {
                                "vec": []
                            }
                        }
                    }
                },
                "type": "write_resource"
            },
            {
                "address": "0xf398b3b71651d2efc73357d1ea7d60023d9f82258e0caa39246b5d86ca98113c",
                "state_key_hash": "0xdc8a899d3f482278c9d724b4267e85f425b0d97816a0d60f6fe5664d0748d196",
                "data": {
                    "type": "0x1::object::ObjectCore",
                    "data": {
                        "allow_ungated_transfer": true,
                        "guid_creation_num": "1125899906842626",
                        "owner": "0x85cafaffb4abd2aef440c7b167ca245b1d7db96d96b22e0553d3173e5491a3d0",
                        "transfer_events": {
                            "counter": "0",
                            "guid": {
                                "id": {
                                    "addr": "0xf398b3b71651d2efc73357d1ea7d60023d9f82258e0caa39246b5d86ca98113c",
                                    "creation_num": "1125899906842624"
                                }
                            }
                        }
                    }
                },
                "type": "write_resource"
            },
            {
                "address": "0xf398b3b71651d2efc73357d1ea7d60023d9f82258e0caa39246b5d86ca98113c",
                "state_key_hash": "0xdc8a899d3f482278c9d724b4267e85f425b0d97816a0d60f6fe5664d0748d196",
                "data": {
                    "type": "0x4::aptos_token::AptosToken",
                    "data": {
                        "burn_ref": {
                            "vec": [
                                {
                                    "inner": {
                                        "vec": [
                                            {
                                                "self": "0xf398b3b71651d2efc73357d1ea7d60023d9f82258e0caa39246b5d86ca98113c"
                                            }
                                        ]
                                    },
                                    "self": {
                                        "vec": []
                                    }
                                }
                            ]
                        },
                        "mutator_ref": {
                            "vec": [
                                {
                                    "self": "0xf398b3b71651d2efc73357d1ea7d60023d9f82258e0caa39246b5d86ca98113c"
                                }
                            ]
                        },
                        "property_mutator_ref": {
                            "self": "0xf398b3b71651d2efc73357d1ea7d60023d9f82258e0caa39246b5d86ca98113c"
                        },
                        "transfer_ref": {
                            "vec": [
                                {
                                    "self": "0xf398b3b71651d2efc73357d1ea7d60023d9f82258e0caa39246b5d86ca98113c"
                                }
                            ]
                        }
                    }
                },
                "type": "write_resource"
            },
            {
                "address": "0xf398b3b71651d2efc73357d1ea7d60023d9f82258e0caa39246b5d86ca98113c",
                "state_key_hash": "0xdc8a899d3f482278c9d724b4267e85f425b0d97816a0d60f6fe5664d0748d196",
                "data": {
                    "type": "0x4::property_map::PropertyMap",
                    "data": {
                        "inner": {
                            "data": [
                                {
                                    "key": "string",
                                    "value": {
                                        "type": 9,
                                        "value": "0x0c737472696e672076616c7565"
                                    }
                                }
                            ]
                        }
                    }
                },
                "type": "write_resource"
            },
            {
                "address": "0xf398b3b71651d2efc73357d1ea7d60023d9f82258e0caa39246b5d86ca98113c",
                "state_key_hash": "0xdc8a899d3f482278c9d724b4267e85f425b0d97816a0d60f6fe5664d0748d196",
                "data": {
                    "type": "0x4::token::Token",
                    "data": {
                        "collection": {
                            "inner": "0x39274b41595692e776b9ebef0911e603980c590c1bbbbb62d1c4b94fb866e803"
                        },
                        "description": "Alice's simple token",
                        "index": "0",
                        "mutation_events": {
                            "counter": "0",
                            "guid": {
                                "id": {
                                    "addr": "0xf398b3b71651d2efc73357d1ea7d60023d9f82258e0caa39246b5d86ca98113c",
                                    "creation_num": "1125899906842625"
                                }
                            }
                        },
                        "name": "",
                        "uri": "https://aptos.dev/img/nyan.jpeg"
                    }
                },
                "type": "write_resource"
            },
            {
                "address": "0xf398b3b71651d2efc73357d1ea7d60023d9f82258e0caa39246b5d86ca98113c",
                "state_key_hash": "0xdc8a899d3f482278c9d724b4267e85f425b0d97816a0d60f6fe5664d0748d196",
                "data": {
                    "type": "0x4::token::TokenIdentifiers",
                    "data": {
                        "index": {
                            "value": "1"
                        },
                        "name": {
                            "padding": "0x00",
                            "value": "Alice's first token"
                        }
                    }
                },
                "type": "write_resource"
            },
            {
                "state_key_hash": "0x6e4b28d40f98a106a65163530924c0dcb40c1349d3aa915d108b4d6cfc1ddb19",
                "handle": "0x1b854694ae746cdbd8d44186ca4929b2b337df21d1c74633be19b2710552fdca",
                "key": "0x0619dc29a0aac8fa146714058e8dd6d2d0f3bdf5f6331907bf91f3acd81e6935",
                "value": "0x2d7afa72490000000100000000000000",
                "data": {
                    "key": "0x619dc29a0aac8fa146714058e8dd6d2d0f3bdf5f6331907bf91f3acd81e6935",
                    "key_type": "address",
                    "value": "18446744389171182125",
                    "value_type": "u128"
                },
                "type": "write_table_item"
            }
        ],
        "sender": "0x85cafaffb4abd2aef440c7b167ca245b1d7db96d96b22e0553d3173e5491a3d0",
        "sequence_number": "1",
        "max_gas_amount": "100000",
        "gas_unit_price": "100",
        "expiration_timestamp_secs": "1721210128",
        "payload": {
            "function": "0x4::aptos_token::mint",
            "type_arguments": [],
            "arguments": [
                "Alice's",
                "Alice's simple token",
                "Alice's first token",
                "https://aptos.dev/img/nyan.jpeg",
                [
                    "string"
                ],
                [
                    "0x1::string::String"
                ],
                [
                    "0x0c737472696e672076616c7565"
                ]
            ],
            "type": "entry_function_payload"
        },
        "signature": {
            "public_key": "0x403aae83b1125983fa7053ebd6a85bbb2e881f74b57206cf0a46acd1783ee948",
            "signature": "0x14d0665cb15ab0fe5aa5787b6732ccba6c1628cd4dbe35ef13df4640b91248c5252cf060115865d282fdd144035957cf840851ed8e8cfade98d8ccf8aac75c0f",
            "type": "ed25519_signature"
        },
        "events": [
            {
                "guid": {
                    "creation_number": "0",
                    "account_address": "0x0"
                },
                "sequence_number": "0",
                "type": "0x4::collection::Mint",
                "data": {
                    "collection": "0x39274b41595692e776b9ebef0911e603980c590c1bbbbb62d1c4b94fb866e803",
                    "index": {
                        "value": "1"
                    },
                    "token": "0xf398b3b71651d2efc73357d1ea7d60023d9f82258e0caa39246b5d86ca98113c"
                }
            },
            {
                "guid": {
                    "creation_number": "0",
                    "account_address": "0x0"
                },
                "sequence_number": "0",
                "type": "0x1::transaction_fee::FeeStatement",
                "data": {
                    "execution_gas_units": "6",
                    "io_gas_units": "5",
                    "storage_fee_octas": "71040",
                    "storage_fee_refund_octas": "0",
                    "total_charge_gas_units": "720"
                }
            }
        ],
        "timestamp": "1721209537854094",
        "type": "user_transaction"
    }
    """;

var j = """
    {
                "guid": {
                    "creation_number": "0",
                    "account_address": "0x0"
                },
                "sequence_number": "0",
                "type": "0x4::collection::Mint",
                "data": {
                    "collection": "0x39274b41595692e776b9ebef0911e603980c590c1bbbbb62d1c4b94fb866e803",
                    "index": {
                        "value": "1"
                    },
                    "token": "0xf398b3b71651d2efc73357d1ea7d60023d9f82258e0caa39246b5d86ca98113c"
                }
            }
    """;

//var eventTx           = JsonConvert.DeserializeObject<TransactionEvent>(j);
//var transactionResult = JsonConvert.DeserializeObject<Transaction>(json, new TransactionConverter());

await AptosToken.RunAptosClientExample();
