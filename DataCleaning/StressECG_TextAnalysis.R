# TEXTMINING X-ECG conclusion

XECG_Conclusion = function(PATIENT_ID, APPOINTMENT_DATE,
                           XECG_REASONSTOP, XECG_REASONSTOPspec, XECG_CONCLUSION)

## CCN Dataset
## Project: Data cleaning CVON-AI
## Initially commenced on 21-8-2019

### ---- INFORMATION --- ###

# Written by Klaske Siegersma
# Testers:
# last update: 13-4-2021

# Description: This script performs simple text retrieval on certain columns of the X-ECG-file of the CCN-dataset.
# Required Files: X-ECG
# Input:
#   PATIENT_ID: Identifier of the individuals in the dataset
#   APPOINTMENT_DATE: Date of the X-ECG
#   XECG_REASONSTOP: Free text field for specificiation of the reason stop. 
#   XECG_REASONSTOPspec: Additional free text field for specification of the reason stop. If this is
#   not present, fill in NA. 
#   XECG_CONCLUSION: Free text of the conclusion of stress test. 
# Output: datafrima with the following columns:
#   PATIENT_NUMBER: Identifier of the individuals in the dataset
#   APPOINTMENT_DATE: Date of the X-ECG
#   XECG_REASONSTOP_HR: Binary indicator that target heart rate has been reached during stress test.
#   XECG_REASONSTOP_DIZZY: Binary indicator that stress test ended because of dizziness.
#   XECG_REASONSTOP_TIRED: Binary indicator that stress test ended because of tiredness.
#   XECG_REASONSTOP_AP: Binary indicator that stress test ended because of chest pain.
#   XECG_REASONSTOP_PAINLEGS: Binary indicator that stress test ended because of pain in the legs.
#   XECG_REASONSTOP_RHYTHM: Binary indicator that stress test ended because of changes in heart rhythm.
#   XECG_REASONSTOP_DYSPNOEA: Binary indicator that stress test ended because of dyspnoea
#   XECG_REASONSTOP_BP: Binary indicator that stress test ended because of blood pressure changes.
# Stress test can be ended because of multiple reasons.
#   XECG_CONCLUSION_NORMAL: Stress test was normal.
#   XECG_CONCLUSION_ABNORMAL: Stress test was abnormal. 
#   XECG_CONCLUSION_INCONCLUSIVE: Stress test was inconclusive.
#   XECG_CONCLUSION_INCOMPLETE: Stress test was incomplete (e.g. target heart rate was not met) 
#   XECG_CONCLUSION_MI: Stress test was stopped due to signs of myocardial infarction.
#   XECG_CONCLUSION_RHYTHM: Stress test was stopped due to rhythm problems. 
# Dictionaries used in this script can be changed according to the wishes of the user. 

{
  # Load libraries
  library(dplyr)
  library(tidytext)
  library(stopwords)
  library(stringr)
  library(readr)
  library(tidyr)
  
  # Initialization of data frames
  if (!is.na(XECG_REASONSTOPspec)){
    # Merge REASONSTOP and REASONSTOPspec into one column
    XECG_REASONSTOPcombined = paste(XECG_REASONSTOP, XECG_REASONSTOPspec, sep = ";")
    df = cbind(PATIENT_ID, APPOINTMENT_DATE, XECG_REASONSTOPcombined, XECG_CONCLUSION)
    df = df %>%
      rename(XECG_REASONSTOP = XECG_REASONSTOPcombined) %>%
      mutate(XECG_REASONSTOP = ifelse(XECG_REASONSTOP == "NA;NA", NA_character_, XECG_REASONSTOP))
  } else {
    df = cbind(PATIENT_ID, APPOINTMENT_DATE, XECG_REASONSTOP, XECG_CONCLUSION)
  }
  
  # Function to evaluate multiple regular expressions/string matchings in one command. 
  MultipleRegEx = function(RegExVector, Data, Data_strings){
    Length = length(RegExVector)
    
    f = str_detect(Data_strings,regex(RegExVector[1],ignore_case = TRUE))
    Selection = Data[which(f==TRUE),]
    if(Length>1){
      for(i in 2:Length){
        f = str_detect(Data_strings,regex(RegExVector[i],ignore_case = TRUE))
        Selection_Interim = Data[which(f==TRUE),]
        Selection = rbind(Selection, Selection_Interim)
      }
    }
    return(Selection)
  }
  
  # INITIALIZATION OF DICTIONARIES ####
  # For reasonstop, tolerance and conclusion. 
  { # Initialize the dictionary for REASONSTOP
    reasonstop = list()
    reasonstop$targetHR = c("behalen target HR", "max hartfrequentie", "max HF", "max HR", "doelbelasting",
                          "max inspanning")
    reasonstop$dizzy = c("duizeligheid", "duizelig", "hoofdpijn", "licht", "hoofd")
    reasonstop$tired = c("vermoeidheid", "conditie", "trappers te zwaar")
    reasonstop$AP = c("pijn op borst", "borst", "beklemmend gevoel", "pob", "druk op de borst")
    reasonstop$painlegs = c("pijn in benen", "knie", "benen", "heup", "been", "verzuring",
                          "vermoeide benen")
    reasonstop$rhythm = c("palpitaties", "een ritmestoornis", "een geleidingsstoornis", "afwijkend XECG", 
                        "depressie", "ECG veranderingen", "hartbonzen", "PVC", "elevatie", "verandering")
    reasonstop$dyspnoea = c("dyspnoe", "ademtekort", "adem tekort", "kortademig", "luchtwegen", "adem")
    reasonstop$BP = c("hypertensie", "hypotensie")
  
    # Initialize the dictionary for CONCLUSION
    conclusion = list()
    conclusion$opt1 = "normaal inspannings ECG"
    conclusion$opt2 = c("abnormaal XECG", "abnormaal inspannings ECG")
    conclusion$opt3 = c("inconclusief", "non-conclusief")
    conclusion$incomplete = c("doelbelasting", "target heart rate")
    conclusion$MI = c("myocardiale ischemie")
    conclusion$rhythm = c("boezemfibrilleren", "VT", "geleidingsstoor", "geleidingsstoornis")
  }
  
  # Initialize dataframe
  FinalDF = data.frame(df$PATIENT_ID, df$APPOINTMENT_DATE, 
                       df$XECG_REASONSTOP, df$XECG_CONCLUSION,
                       matrix(NA, nrow = length(data_xecg$PATIENT_NUMBER), ncol = 20))
  colnames(FinalDF) = c("PATIENT_NUMBER", "APPOINTMENT_DATE", "XECG_REASONSTOP", "XECG_CONCLUSION",
                        "XECG_REASONSTOP_HR", "XECG_REASONSTOP_DIZZY", "XECG_REASONSTOP_TIRED",
                        "XECG_REASONSTOP_AP", "XECG_REASONSTOP_PAINLEGS", "XECG_REASONSTOP_RHYTHM",
                        "XECG_REASONSTOP_DYSPNOEA", "XECG_REASONSTOP_BP",
                        "XECG_CONCLUSION_NORMAL", "XECG_CONCLUSION_ABNORMAL", "XECG_CONCLUSION_INCONCLUSIVE",
                        "XECG_CONCLUSION_INCOMPLETE", "XECG_CONCLUSION_MI", "XECG_CONCLUSION_RHYTHM")
  rownames(FinalDF) = df$PATIENT_ID


{ # XECG-REASONSTOP TEXT RETRIEVAL ####
  { # Set all reason stops that have been filled in to 0 --> 
    # Something has been filled in, so no missings. 
    # However, changing what has been filled in, will be done during the following analysis. 
    FinalDF[which(!is.na(FinalDF$XECG_REASONSTOP)),
            c("XECG_REASONSTOP_HR", "XECG_REASONSTOP_DIZZY","XECG_REASONSTOP_TIRED", 
              "XECG_REASONSTOP_AP", "XECG_REASONSTOP_PAINLEGS", "XECG_REASONSTOP_RHYTHM", 
              "XECG_REASONSTOP_DYSPNOEA", "XECG_REASONSTOP_BP")] = 0
  }
  
  { # Target HR
    df_targetHR = MultipleRegEx(reasonstop$targetHR, FinalDF, FinalDF$XECG_REASONSTOP)
    FinalDF[as.character(unique(df_targetHR$PATIENT_ID)),"XECG_REASONSTOP_HR"] = 1 
  }
  
  { # Dizzy
    df_dizzy = MultipleRegEx(reasonstop$dizzy , FinalDF, FinalDF$XECG_REASONSTOP)
    FinalDF[as.character(unique(df_dizzy$PATIENT_ID)),"XECG_REASONSTOP_DIZZY"] = 1 
  }
  
  { # Tired
    df_tired = MultipleRegEx(reasonstop$tired , FinalDF, FinalDF$XECG_REASONSTOP)
    FinalDF[as.character(unique(df_tired$PATIENT_ID)),"XECG_REASONSTOP_TIRED"] = 1 
  }
  
  { # AP
    df_AP = MultipleRegEx(reasonstop$AP , FinalDF, FinalDF$XECG_REASONSTOP)
    FinalDF[as.character(unique(df_AP$PATIENT_ID)),"XECG_REASONSTOP_AP"] = 1 
  }
  
  { # Pain legs
    df_painlegs = MultipleRegEx(reasonstop$painlegs , FinalDF, FinalDF$XECG_REASONSTOP)
    FinalDF[as.character(unique(df_painlegs$PATIENT_ID)),"XECG_REASONSTOP_PAINLEGS"] = 1 
  }
  
  { # Rhythm
    df_rhythm = MultipleRegEx(reasonstop$AP , FinalDF, FinalDF$XECG_REASONSTOP)
    FinalDF[as.character(unique(df_rhythm$PATIENT_ID)),"XECG_REASONSTOP_RHYTHM"] = 1 
  }
  
  { # Dyspnoea
    df_dyspnoea = MultipleRegEx(reasonstop$AP , FinalDF, FinalDF$XECG_REASONSTOP)
    FinalDF[as.character(unique(df_dyspnoea$PATIENT_ID)),"XECG_REASONSTOP_DYSPNOEA"] = 1 
  }
  
  { # Bloodpressure
    df_BP = MultipleRegEx(reasonstop$AP , FinalDF, FinalDF$XECG_REASONSTOP)
    FinalDF[as.character(unique(df_BP$PATIENT_ID)),"XECG_REASONSTOP_BP"] = 1 
  }
  
  { # Set all the reason-stop columns to -1 if no reasonstop has been filled in:
    FinalDF[which(is.na(FinalDF$XECG_REASONSTOPcombined)),
            c("XECG_REASONSTOP_HR", "XECG_REASONSTOP_DIZZY","XECG_REASONSTOP_TIRED", 
              "XECG_REASONSTOP_AP", "XECG_REASONSTOP_PAINLEGS", "XECG_REASONSTOP_RHYTHM", 
              "XECG_REASONSTOP_DYSPNOEA", "XECG_REASONSTOP_BP")] = -1
  }
}

{ # XECG-CONCLUSION TEXT RETRIEVAL ####
    # Conclusion: normal  
    df_normal = MultipleRegEx(conclusion$opt1, FinalDF, FinalDF$XECG_CONCLUSION)
    FinalDF[as.character(unique(df_normal$PATIENT_ID)),"XECG_CONCLUSION_NORMAL"] = 1 
  
    # Conclusion: abnormal  
    df_abnormal = MultipleRegEx(conclusion$opt2, FinalDF, FinalDF$XECG_CONCLUSION)
    FinalDF[as.character(unique(df_abnormal$PATIENT_ID)),"XECG_CONCLUSION_ABNORMAL"] = 1 
  
    # Conclusion: Inconclusive
    df_inconclusive = MultipleRegEx(conclusion$opt3, FinalDF, FinalDF$XECG_CONCLUSION)
    FinalDF[as.character(unique(df_inconclusive$PATIENT_ID)),"XECG_CONCLUSION_INCONCLUSIVE"] = 1 
  
    # Conclusion: THR not reached
    df_incomplete = MultipleRegEx(conclusion$incomplete, FinalDF, FinalDF$XECG_CONCLUSION)
    FinalDF[as.character(unique(df_incomplete$PATIENT_ID)),"XECG_CONCLUSION_INCOMPLETE"] = 1 
 
    # Conclusion: MI
    df_MI = MultipleRegEx(conclusion$MI, FinalDF, FinalDF$XECG_CONCLUSION)
    FinalDF[as.character(unique(df_MI$PATIENT_ID)),"XECG_CONCLUSION_MI"] = 1 
  
    # Conclusion: Rhythm problems
    df_rhythm = MultipleRegEx(conclusion$rhythm, FinalDF, FinalDF$XECG_CONCLUSION)
    FinalDF[as.character(unique(df_rhythm$PATIENT_ID)),"XECG_CONCLUSION_RHYTHM"] = 1
  
    # OPTIONS! FOR CONCLUSION 
    # Set to -1 if no conclusion has been filled in. 
    FinalDF[which(is.na(FinalDF$XECG_CONCLUSION)), 
          c("XECG_CONCLUSION_NORMAL", "XECG_CONCLUSION_ABNORMAL", "XECG_CONCLUSION_INCONCLUSIVE",
          "XECG_CONCLUSION_INCOMPLETE", "XECG_CONCLUSION_MI", "XECG_CONCLUSION_RHYTHM")] = -1 
    # Set conclusion to normal if no conclusion has been filled in.  
    FinalDF[which(is.na(FinalDF$XECG_CONCLUSION)), 
          c("XECG_CONCLUSION_NORMAL")] = 1
}
  
  return(FinalDF)
} 

