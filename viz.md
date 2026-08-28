digraph G {
  node [shape=rect];

  subgraph cluster_0 {
    node [style=filled];
    mconsume, mprod -> mrabbit;
    mweb -> mdb;
    label = "MESH";
    color=blue
  } 

  subgraph cluster_1 {
    node [style=filled];
    consume -> db
    label = "Aberystwyth";
    color=blue
  }

    subgraph cluster_2 {
    node [style=filled];
    prod, web;
    label = "Bangor";
    color=blue
  }

  subgraph cluster_3 {
    node [style=filled];
    rabbit;
    label = "Cardiff";
    color=blue
  }

  consume -> mconsume;
  mdb -> db;
  web -> mweb;
  prod -> mprod;
  mrabbit -> rabbit;
}

https://www.devtoolsdaily.com/graphviz/?#v2=N4IgJg9gLiBc4EsDmAnAhgBwBYAIDiOwAOgHY44kRgCmOA2gM5abUC8K1AxlALoDcpUuQYBXAEapMuTgBsRDKNRQB9AAyEh5ClVqMoATxlsAZghlGw-TeQC2nCCVE3qAGhw2MKKjgC0APnd0MTEEKAEyLRsAd2oxXwCbMDFwrRwZNDFqGRxWHCIQAFkAUQBlAAl8lK17GQgUVjE5ak0AXzySTVEJdGwcWXlFFQBGDQjyShp6BUMTMwsrMb6HJ1p-HCTrNIysnLyQAEFMlH0FKP0oLErNmrqGptbBRa7JXv6FJWUAJlHUid1poysUzmaiWKrkTxUNwxZKbdKZbK5fIAITQJCQdSuixu9UaImaERaj2E4he0jk7xUAGYflo-lMDIDgfNwTggiEwnDtoi9gBhNAoMAIYzGLGpHF3fEPDoReyOETOeLuOUrKqJOJrJJVGFK6KxKqQsC6w1q9mhJVmzkkFogFpAA