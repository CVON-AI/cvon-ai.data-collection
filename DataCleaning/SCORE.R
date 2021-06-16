calculate_SCORE = function(PATIENT_ID, Sex, Age, 
                           Current_Smoker, Lab_Chol_Total, SBP){
  # calculate SCORE value for each participant
  # source: https://academic.oup.com/eurheartj/article/24/11/987/427645 
  
  # Input: 
  #   Current_Smoker: 0 or 1 for non-smoker/smoker
  #   Sex: V is for female, M for male. 
  #   Age, Lab_Chol_Total and SBP can be numeric values of age, total cholesterol and systolic blood pressure.
  # Preferably, all variables come from the same dataframe. Make sure that the rows correspond to the same patient 
  # in each variable. 
  
  # Output: Dataframe that contains:
  #   - PATIENT_NUMBER: Identification of each individual.
  #   - SCORE_CVD: The SCORE-risk
  
  # Step 1: Calculate underlying survival probability based on age
  # using low-risk score, seems to be standard clinical use in NL : https://www.ncbi.nlm.nih.gov/pmc/articles/PMC5313447/
  # stratified by sex, so ifelse gives option for sex == V and if it's not (aka sex == M)
  # risk of CHD and non-CHD CVD are calculated separately and merged in the end
  
  
  pat_base = cbind.data.frame(PATIENT_ID, Sex, Age, Current_Smoker, 
                              Lab_Chol_Total, 
                              SBP)
  colnames(pat_base) = c("PATIENT_NUMBER", "Sex", "AGE", "CURRENT_SMOKER","LAB_CHOL_TOT", "SBP")
  num = c("AGE", "LAB_CHOL_TOT", "SBP", "CURRENT_SMOKER")
  factor = c("Sex")
  to_numeric = function(x){
    as.numeric(as.character(x))
  }
  pat_base[,num] = lapply(pat_base[,num], to_numeric)

  # - CHD
  pat_base$S0CHD_age <- ifelse(pat_base$Sex == "V",
                               exp(-(exp(-29.8)) * ((pat_base$AGE - 20) ** 6.36)),
                               exp(-(exp(-22.1)) * ((pat_base$AGE - 20) ** 4.71)))
  
  pat_base$S0CHD_age10 <- ifelse(pat_base$Sex == "V",
                                 exp(-(exp(-29.8)) * ((pat_base$AGE - 10) ** 6.36)),
                                 exp(-(exp(-22.1)) * ((pat_base$AGE - 10) ** 4.71)))
  
  # - nonCHD
  pat_base$S0nonCHD_age <- ifelse(pat_base$Sex == "V",
                                  exp(-(exp(-31.0)) * ((pat_base$AGE - 20) ** 6.62)),
                                  exp(-(exp(-26.7)) * ((pat_base$AGE - 20) ** 5.64)))
  
  pat_base$S0nonCHD_age10 <- ifelse(pat_base$Sex == "V",
                                    exp(-(exp(-31.0)) * ((pat_base$AGE - 10) ** 6.62)),
                                    exp(-(exp(-26.7)) * ((pat_base$AGE - 10) ** 5.64)))
  
  # Step 2: Calculate weighted sum of risk factors
  # - CHD
  pat_base$wCHD <- (0.71 * pat_base$CURRENT_SMOKER) +
    (0.24 * (pat_base$LAB_CHOL_TOT-6)) +
    (0.018 * (pat_base$SBP-120))
  
  # - nonCHD
  pat_base$wnonCHD <- (0.63 * pat_base$CURRENT_SMOKER) +
    (0.02 * (pat_base$LAB_CHOL_TOT-6)) +
    (0.022 * (pat_base$SBP-120))
  
  # Step 3: Combine step 1 & 2, calculate probability of survival at each age
  
  # - CHD
  pat_base$SCHD_age <- pat_base$S0CHD_age ** exp(pat_base$wCHD)
  pat_base$SCHD_age10 <- pat_base$S0CHD_age10 ** exp(pat_base$wCHD)
  
  # - nonCHD
  pat_base$SnonCHD_age <- pat_base$S0nonCHD_age ** exp(pat_base$wnonCHD)
  pat_base$SnonCHD_age10 <- pat_base$S0nonCHD_age10 ** exp(pat_base$wnonCHD)
  
  # Step 4: Calculate 10-year survival probability
  
  # - CHD
  pat_base$S10CHD <- pat_base$SCHD_age10/pat_base$SCHD_age
  
  # - nonCHD
  pat_base$S10nonCHD <- pat_base$SnonCHD_age10/pat_base$SnonCHD_age
  
  # Step 5: Calculate 10-year risk
  pat_base$Risk_CHD10 <- 1 - pat_base$S10CHD
  pat_base$Risk_nonCHD10 <- 1 - pat_base$S10nonCHD
  
  # Step 6: Combine risks for CHD and non-CHD CVD
  pat_base$SCORE_CVD <- pat_base$Risk_CHD10 + pat_base$Risk_nonCHD10 
  pat_base$SCORE_CVD <- pat_base$SCORE_CVD * 100
  
  # QC
  
  # SCORE cannot be larger than 100 or smaller than 0
  # so create 1) checkup var to see which patients have these weird values
  pat_base$SCORE_check <- ifelse(pat_base$SCORE_CVD > 100 |
                                   pat_base$SCORE_CVD < 0, 1, 0)
  
  # check-up shows some people with SCORE > 100
  # these are old people (age > 80) with high SBP (> 185) and relatively high total cholesterol (> 5)
  # so these are at high risk of dying because of old age, recode to the maximum of 100
  pat_base$SCORE_CVD <- ifelse(pat_base$SCORE_CVD > 100,
                               100, 
                               pat_base$SCORE_CVD)
  
  return(pat_base[,c("PATIENT_NUMBER", "SCORE_CVD")])
  
  
}

