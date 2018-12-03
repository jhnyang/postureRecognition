import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import pickle

from sklearn.model_selection import train_test_split
from sklearn import svm, metrics
from sklearn.naive_bayes import GaussianNB
from sklearn.neighbors import KNeighborsClassifier
from sklearn.tree import DecisionTreeClassifier
from sklearn.ensemble import RandomForestClassifier, AdaBoostClassifier

data_sit = pd.read_csv(r'C:\Users\Jhnya\Desktop\capstone\sitDB.csv')
data_stand = pd.read_csv(r'C:\Users\Jhnya\Desktop\capstone\standDB.csv')
data_lying = pd.read_csv(r'C:\Users\Jhnya\Desktop\capstone\lyingDB.csv')

data_sit.iloc[:,::3]=data_sit.iloc[:,0::3].sub(data_sit.loc[:,'SpineBase_x'], axis=0)
data_sit.iloc[:,1::3] =data_sit.iloc[:,1::3].sub(data_sit.loc[:,'SpineBase_y'], axis=0)
data_sit.iloc[:,2::3] =data_sit.iloc[:,2::3].sub(data_sit.loc[:,'SpineBase_z'], axis=0)

data_stand.iloc[:,::3]=data_stand.iloc[:,0::3].sub(data_stand.loc[:,'SpineBase_x'], axis=0)
data_stand.iloc[:,1::3] =data_stand.iloc[:,1::3].sub(data_stand.loc[:,'SpineBase_y'], axis=0)
data_stand.iloc[:,2::3] =data_stand.iloc[:,2::3].sub(data_stand.loc[:,'SpineBase_z'], axis=0)

data_lying.iloc[:,::3]=data_lying.iloc[:,0::3].sub(data_lying.loc[:,'SpineBase_x'], axis=0)
data_lying.iloc[:,1::3] =data_lying.iloc[:,1::3].sub(data_lying.loc[:,'SpineBase_y'], axis=0)
data_lying.iloc[:,2::3] =data_lying.iloc[:,2::3].sub(data_lying.loc[:,'SpineBase_z'], axis=0)


data_sit["posture"]=0
data_stand['posture']=1
data_lying['posture']=2

data_tot = pd.concat([data_sit, data_stand, data_lying])

label = data_tot['posture']
data= data_tot.iloc[:,:-1]

x_train, x_test, y_train, y_test = train_test_split(data, label, test_size=0.15, random_state=42)

test_tuple = (x_test, y_test)
pickle.dump(test_tuple, open("test_data.pk1",'wb'))

naive_G = GaussianNB()
naive_G.fit(x_train,y_train)

knn = KNeighborsClassifier(n_neighbors=5)
knn.fit(x_train,y_train)

svm_clf = svm.SVC(gamma=2, C=1)
svm_clf.fit(x_train,y_train)

ran_forest = RandomForestClassifier(max_depth=5, n_estimators=10, max_features=1)
ran_forest.fit(x_train,y_train)

desisionT= DecisionTreeClassifier(max_depth=5)
desisionT.fit(x_train,y_train)

model_objects = (naive_G, knn, svm_clf, ran_forest, desisionT)
pickle.dump(model_objects, open("models.pk1", 'wb'))




