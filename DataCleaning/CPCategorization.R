CharacterizeCP = function(df, id, CPtype = NA, radiation = NA, provocation = NA, 
                          allevation = NA){
  # Initialization
  library(stringr)
  library(tidyr)
  library(dplyr)
  
  return_df = data.frame(df[,id])
  
  dict_CP = list(
    "CPtype" = c("kramp", "knijp", "druk", "steen", "band", "beklem", "snoer", "elast"), 
    "CPradiation" = c("arm", "kaak", "nek", "schouder"), 
    "CPstarttype" = c("acuut", "progressief"),
    "CPstarttype_neg" = c("geleidelijk"),
    "CPprovocation" = c("inspanning", "lopen", "sport", "wandelen", "trap", 
                        "stress","spanning", "druk", "schrik", "emotie", "kou", "warm"),
    "CPalleviation" = c("ontspannen", "slapen", "yoga", "rust", "verlagen", "verminderen", "zitten", 
                        "NTG", "nitro"),
    "CPvegetative" = c("angst", "bleek", "duizelig", "zweten", "transpireren", "misselijk", "palpitaties", "klop", 
                       "vegetatieve", "zwak"),
    "CPdyspnoea" = c("kortademig", "dyspnoe", "benau"), 
    "CPNYHA" = c("II", "III", "IV", "2", "3", "4"),
    "CPnegation" = c("afwezig", "geen", "niet")
  )

  if(!is.na(CPtype)){
    
    vector_regex = df %>%
      pull(CPtype) %>%
      str_detect(., regex(dict_CP$CPtype, ignore_case = TRUE))
    
    return_df = cbind(return_df, vector_regex)
    colnames(return_df)[2] = "CP.pressure"
    }

  if(!is.na(radiation)){
    
    vector_regex1 = df %>%
      pull(radiation) %>%
      str_detect(., regex(dict_CP$CPradiation, ignore_case = TRUE))
    vector_regex2 = df %>%
      pull(radiation) %>%
      str_detect(., regex(dict_CP$CPnegation, ignore_case = TRUE))
    
    vector_regex = cbind(vector_regex1, vector_regex2)
    vector_regex = case_when(
      vector_regex1 == TRUE & vector_regex2 == TRUE ~ FALSE, # negation of radiation
      vector_regex1 == FALSE & vector_regex2 == TRUE ~ FALSE, 
      vector_regex1 == FALSE & vector_regex2 == FALSE ~ TRUE, # radiation present
      vector_regex1 == TRUE & vector_regex2 == FALSE ~ TRUE, 
    )
    
    return_df = cbind(return_df, vector_regex)
    colnames(return_df)[3] = "CP.radiation"
  }
  
  if(!is.na(provocation)){
    # PROVOCATION (CP_AGGRAVATE) - Free text. 
    # NHG-guidelines - uitlokkende factoren: inspanning, emoties, kou, warmte (stable AP)
    # Currently this free text has been coded into one single feature.
    
    vector_regex1 = df %>%
      pull(provocation) %>%
      str_detect(., regex(dict_CP$CPprovocation, ignore_case = TRUE))
    vector_regex2 = df %>%
      pull(provocation) %>%
      str_detect(., regex(dict_CP$CPnegation, ignore_case = TRUE))
    
    vector_regex = cbind(vector_regex1, vector_regex2)
    vector_regex = case_when(
      vector_regex1 == TRUE & vector_regex2 == TRUE ~ FALSE, # negation of provocation
      vector_regex1 == FALSE & vector_regex2 == TRUE ~ FALSE, 
      vector_regex1 == FALSE & vector_regex2 == FALSE ~ TRUE, # provocation present
      vector_regex1 == TRUE & vector_regex2 == FALSE ~ TRUE, 
    )
    
    return_df = cbind(return_df, vector_regex)
    colnames(return_df)[4] = "CP.provocation"
  }
  
  if(!is.na(alleviation)){
    
    # ALLEVIATION (CP_ALLEVIATON) - Free text.
    # NHG-guidelines - Typical AP: Alleviation with rest within 14 minutes or through the use of NTG. 
    
    vector_regex = df %>%
      pull(alleviation) %>%
      str_detect(., regex(dict_CP$CPalleviation, ignore_case = TRUE))
    
    return_df = cbind(return_df, vector_regex)
    colnames(return_df)[5] = "CP.alleviation"
  }
  
  # Categorical analysis of chestpain ####
  # 3 complaints: 1. CP_COMPLAINTS, 2. CP_PROVOCATION, 3. CP_ALLEVIATION
  # Typical CP: all 3 == 1
  # Atypical CP: 2 complaints == 1
  # Aspecific: 1 or less == 1 
  
  return_df = return_df %>%
    mutate_at(vars(CP.pressure, CP.provocation, CP.alleviation), 
              ~replace_na(., FALSE)) %>%
    mutate(CP.sum = CP.pressure + CP.provocation + CP.alleviation) %>%
    mutate(CP.categorization = case_when(
      CP.sum == 3 ~ "Typical", 
      CP.sum == 2 ~ "Atypical", 
      CP.sum == 1 | CP.sum == 0 ~ "Aspecific"
    ))
  
  return(return_df)
  

  FinalDF$SUM_CHARACTERIZATION = rowSums(FinalDF[,c("CP_PRESSURE", "CP_PROVOCATION", "CP_ALLEVIATION")])
  FinalDF[which(FinalDF$SUM_CHARACTERIZATION == 3),"CP_CATEGORIZATION"] = "Typical"
  FinalDF[which(FinalDF$SUM_CHARACTERIZATION == 2),"CP_CATEGORIZATION"] = "Atypical"
  FinalDF[which(FinalDF$SUM_CHARACTERIZATION <= 1),"CP_CATEGORIZATION"] = "Aspecific"
  
  if(!is.na())
    
}
