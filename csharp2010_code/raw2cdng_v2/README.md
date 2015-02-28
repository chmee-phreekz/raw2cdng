#raw2cdng - changelog
***
**1.7.0**   
*full mlv and raw support with new io-module - io.cs readchunk()   
*fixed raw-splitfiles-bug - io.cs readchunk()   
*vertical banding optimized - calc.cs   
*modelname, if not found/wrong in mlv, will be recreated from colormatrix-values @Walter_Schulz   
*fps-tag - dropped frames are now well-formed (25000/1001 instead of 24975/1000)   
***
**1.6.9.alpha**  
*read-io rewritten - io.cs readchunk()   
*if modelname faulty, reconstruct from colormatrix io.cs getMLVAttributes()   
*rewrite dropped fps - io.cs getMLVAttributes() and getRAWAttributes()   
***
regards chmee